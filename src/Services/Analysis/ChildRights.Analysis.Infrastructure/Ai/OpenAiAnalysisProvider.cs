using System.Net.Http.Json;
using System.Text.Json;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Enums;
using ChildRights.Analysis.Domain.Rules;
using ChildRights.Analysis.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChildRights.Analysis.Infrastructure.Ai;

/// <summary>
/// LLM-backed analysis provider (OpenAI-compatible chat completions). Demonstrates the
/// AI best practices baked into the platform:
///   • data minimisation — no personal identifiers are sent to the model;
///   • a strict JSON response contract that is parsed defensively;
///   • resilience — the HTTP client uses a standard retry/timeout/circuit-breaker handler;
///   • graceful degradation — any failure falls back to the deterministic rule engine.
/// </summary>
internal sealed class OpenAiAnalysisProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<AiOptions> options,
    ILogger<OpenAiAnalysisProvider> logger) : IAiAnalysisProvider
{
    public const string HttpClientName = "openai-analysis";

    private const string SystemPrompt =
        "Ти — асистент аналітики прав дитини для українських шкіл. " +
        "Проаналізуй знеособлені дані учня і поверни ВИКЛЮЧНО JSON-обʼєкт такого вигляду: " +
        "{\"flags\":[{\"ruleCode\":string,\"severity\":\"Green|Yellow|Orange|Red\"," +
        "\"title\":string,\"description\":string," +
        "\"sourceAgency\":\"Education|SocialServices|Medical|JuvenilePolice\"," +
        "\"targetAudiences\":[\"ClassTeacher|Parent|SchoolAdministration|SocialService|JuvenilePolice|MedicalService|Student\"]," +
        "\"recommendedActions\":[string]}]," +
        "\"recommendations\":[{\"kind\":\"ProfileChoice|ProfileChange|OpenProfile|CloseProfile|AcademyCourse\"," +
        "\"title\":string,\"summary\":string,\"rationale\":string,\"confidence\":number}]," +
        "\"summary\":string}. Текстові поля — українською мовою.";

    public string ModelName => options.Value.OpenAi.Model;

    public async Task<AnalysisResult> AnalyzeAsync(AnalysisRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            var payload = new
            {
                model = options.Value.OpenAi.Model,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = BuildUserPrompt(request.Snapshot) }
                }
            };

            using var response = await client.PostAsJsonAsync(options.Value.OpenAi.Endpoint, payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Parse(content ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LLM analysis failed; falling back to the deterministic rule engine.");
            var evaluation = StudentRiskRules.Evaluate(request.Snapshot);
            return new AnalysisResult(
                ModelName + "+fallback",
                evaluation.Flags,
                evaluation.Recommendations,
                "Модель ШІ недоступна — застосовано детерміновані правила.");
        }
    }

    // Personal identifiers are deliberately excluded from the prompt (data minimisation).
    private static string BuildUserPrompt(StudentSnapshot snapshot)
    {
        var subjects = string.Join(", ", snapshot.SubjectAverages.Select(kv => $"{kv.Key}: {kv.Value:0.0}"));
        var topics = string.Join(", ", snapshot.TopicAverages.Select(t => $"{t.Subject}/{t.Topic}: {t.Average:0.0}"));
        var desired = snapshot.DesiredProfiles.Count > 0
            ? string.Join(", ", snapshot.DesiredProfiles)
            : "немає";

        return
            $"Клас (рік навчання): {snapshot.GradeLevel}. " +
            $"Поточний профіль: {snapshot.DeclaredProfile?.ToString() ?? "немає"}. " +
            $"Бажані профілі: {desired}. " +
            $"Пропуски без поважної причини: {snapshot.UnexcusedAbsences}. " +
            $"Середні бали за предметами: {subjects}. " +
            $"Середні бали за темами: {topics}.";
    }

    private AnalysisResult Parse(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var flags = new List<FlagFinding>();
        if (root.TryGetProperty("flags", out var flagsElement) && flagsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in flagsElement.EnumerateArray())
            {
                flags.Add(new FlagFinding(
                    GetString(element, "ruleCode", "AI-FLAG"),
                    ParseEnum(GetString(element, "severity", "Yellow"), FlagSeverity.Yellow),
                    GetString(element, "title", "Сигнал ШІ"),
                    GetString(element, "description", string.Empty),
                    ParseEnum(GetString(element, "sourceAgency", "Education"), Agency.Education),
                    GetStringArray(element, "targetAudiences").Select(a => ParseEnum(a, AudienceRole.ClassTeacher)).ToList(),
                    GetStringArray(element, "recommendedActions")));
            }
        }

        var recommendations = new List<RecommendationFinding>();
        if (root.TryGetProperty("recommendations", out var recsElement) && recsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in recsElement.EnumerateArray())
            {
                recommendations.Add(new RecommendationFinding(
                    ParseEnum(GetString(element, "kind", "ProfileChoice"), RecommendationKind.ProfileChoice),
                    GetString(element, "title", "Рекомендація"),
                    GetString(element, "summary", string.Empty),
                    GetString(element, "rationale", string.Empty),
                    element.TryGetProperty("confidence", out var c) && c.TryGetDouble(out var confidence) ? confidence : 0.6));
            }
        }

        var summary = GetString(root, "summary", $"Модель {ModelName}: {flags.Count} флагів, {recommendations.Count} рекомендацій.");
        return new AnalysisResult(ModelName, flags, recommendations, summary);
    }

    private static string GetString(JsonElement element, string property, string fallback) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;

    private static List<string> GetStringArray(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToList();
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct =>
        Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : fallback;
}
