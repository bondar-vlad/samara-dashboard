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

    // Shared red-flag JSON shape, reused by every goal's contract.
    private const string FlagSchema =
        "{\"ruleCode\":string,\"severity\":\"Green|Yellow|Orange|Red\",\"title\":string," +
        "\"description\":string,\"sourceAgency\":\"Education|SocialServices|Medical|JuvenilePolice\"," +
        "\"targetAudiences\":[\"ClassTeacher|Parent|SchoolAdministration|SocialService|JuvenilePolice|MedicalService|Student\"]," +
        "\"recommendedActions\":[string]}";

    /// <summary>
    /// One dedicated system instruction per <see cref="AnalysisGoal"/>. Each prompt narrows the
    /// model to a single concrete case — its own red flags, its own recommendation kind and JSON
    /// contract — instead of asking one prompt to do everything at once. This mirrors the rule
    /// engine's per-goal split and lets each goal later become its own model.
    /// </summary>
    private static readonly IReadOnlyDictionary<AnalysisGoal, string> SystemPrompts =
        new Dictionary<AnalysisGoal, string>
        {
            [AnalysisGoal.StudentRisk] =
                "Ти — асистент моніторингу прав і добробуту учня українських шкіл. " +
                "Твоя ЄДИНА ціль — виявити РЕД-ФЛАГИ ризику за знеособленими даними: систематичні " +
                "пропуски без поважної причини та падіння успішності. Не давай порад щодо профілю чи вступу. " +
                "Поверни ВИКЛЮЧНО JSON: {\"flags\":[" + FlagSchema + "],\"summary\":string}. Текст — українською.",

            [AnalysisGoal.ProfileChoice] =
                "Ти — радник з вибору профілю навчання у 10 класі (реформа НУШ). " +
                "Твоя ЄДИНА ціль — за сильними предметами й темами порекомендувати профільний кластер і " +
                "конкретні профілі та ПОЗНАЧИТИ РЕД-ФЛАГ, якщо бажаний учнем профіль не відповідає даним. " +
                "Поверни ВИКЛЮЧНО JSON: {\"recommendations\":[{\"kind\":\"ProfileChoice|ProfileChange|OpenProfile|CloseProfile\"," +
                "\"cluster\":string,\"profiles\":[string],\"title\":string,\"summary\":string,\"rationale\":string," +
                "\"confidence\":number}],\"flags\":[" + FlagSchema + "],\"summary\":string}. Текст — українською.",

            [AnalysisGoal.NmtFourthSubject] =
                "Ти — радник з вибору 4-го предмета НМТ для учня 11 класу. " +
                "Твоя ЄДИНА ціль — за сильними предметами й темами порекомендувати оптимальний 4-й предмет НМТ " +
                "та ПОЗНАЧИТИ РЕД-ФЛАГ, якщо обраний учнем предмет не відповідає його результатам. " +
                "Поверни ВИКЛЮЧНО JSON: {\"recommendations\":[{\"kind\":\"FourthSubjectChoice\",\"title\":string," +
                "\"summary\":string,\"rationale\":string,\"confidence\":number}],\"flags\":[" + FlagSchema + "]," +
                "\"summary\":string}. Текст — українською.",

            [AnalysisGoal.AdmissionDirection] =
                "Ти — радник з вибору напряму вступу (фаху) для учня 11 класу. " +
                "Твоя ЄДИНА ціль — за балами НМТ та профільними темами порекомендувати напрям вступу з наведеного " +
                "переліку та ПОЗНАЧИТИ РЕД-ФЛАГ, якщо бажаний учнем напрям не відповідає його результатам. " +
                "Поверни ВИКЛЮЧНО JSON: {\"recommendations\":[{\"kind\":\"AdmissionDirectionChoice\",\"directionCode\":string," +
                "\"title\":string,\"summary\":string,\"rationale\":string,\"confidence\":number}],\"flags\":[" + FlagSchema + "]," +
                "\"summary\":string}. Текст — українською."
        };

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
                    new { role = "system", content = SystemPromptFor(request.Goal) },
                    new { role = "user", content = BuildUserPrompt(request) }
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

            return Parse(content ?? string.Empty, request.Goal);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LLM analysis failed; falling back to the deterministic rule engine.");
            var evaluation = RuleBasedAiAnalysisProvider.EvaluateGoal(request);
            return new AnalysisResult(
                ModelName + "+fallback",
                evaluation.Flags,
                evaluation.Recommendations,
                "Модель ШІ недоступна — застосовано детерміновані правила.");
        }
    }

    private static string SystemPromptFor(AnalysisGoal goal) =>
        SystemPrompts.TryGetValue(goal, out var prompt) ? prompt : SystemPrompts[AnalysisGoal.StudentRisk];

    // Personal identifiers are deliberately excluded from every prompt (data minimisation).
    // Each goal sees only the facts relevant to its concrete case.
    private static string BuildUserPrompt(AnalysisRequest request)
    {
        var snapshot = request.Snapshot;
        return request.Goal switch
        {
            AnalysisGoal.ProfileChoice => BuildProfileChoicePrompt(snapshot),
            AnalysisGoal.NmtFourthSubject => BuildFourthSubjectPrompt(snapshot, request.Admission),
            AnalysisGoal.AdmissionDirection => BuildDirectionPrompt(snapshot, request.Admission),
            _ => BuildRiskPrompt(snapshot)
        };
    }

    private static string Subjects(StudentSnapshot s) =>
        string.Join(", ", s.SubjectAverages.Select(kv => $"{kv.Key}: {kv.Value:0.0}"));

    private static string Topics(StudentSnapshot s) =>
        string.Join(", ", s.TopicAverages.Select(t => $"{t.Subject}/{t.Topic}: {t.Average:0.0}"));

    private static string BuildRiskPrompt(StudentSnapshot s) =>
        $"Клас (рік навчання): {s.GradeLevel}. " +
        $"Пропуски без поважної причини: {s.UnexcusedAbsences}. " +
        $"Середні бали за предметами: {Subjects(s)}. " +
        $"Середні бали за темами: {Topics(s)}.";

    private static string BuildProfileChoicePrompt(StudentSnapshot s)
    {
        var declared = s.DeclaredProfile is { } d ? ProfileTaxonomy.Localize(d) : "немає";
        var desired = s.DesiredProfiles.Count > 0
            ? string.Join(", ", s.DesiredProfiles.Select(ProfileTaxonomy.Localize))
            : "немає";

        return
            $"Клас (рік навчання): {s.GradeLevel}. " +
            $"Поточний профіль: {declared}. " +
            $"Бажані профілі: {desired}. " +
            $"Середні бали за предметами: {Subjects(s)}. " +
            $"Середні бали за темами: {Topics(s)}.";
    }

    private static string BuildFourthSubjectPrompt(StudentSnapshot s, AdmissionInputs? admission)
    {
        var chosen = admission?.ChosenFourthSubject is { } c ? NmtSubjectCatalog.Localize(c) : "не обрано";
        var optionList = string.Join(", ", NmtSubjectCatalog.FourthSubjectOptions.Select(NmtSubjectCatalog.Localize));

        return
            $"Клас (рік навчання): {s.GradeLevel}. " +
            $"Обраний 4-й предмет НМТ: {chosen}. " +
            $"Доступні варіанти 4-го предмета: {optionList}. " +
            $"Середні бали за предметами: {Subjects(s)}. " +
            $"Середні бали за темами: {Topics(s)}.";
    }

    private static string BuildDirectionPrompt(StudentSnapshot s, AdmissionInputs? admission)
    {
        var nmt = admission is { NmtScores.Count: > 0 }
            ? string.Join(", ", admission.NmtScores.Select(kv => $"{NmtSubjectCatalog.Localize(kv.Key)}: {kv.Value}"))
            : "немає";
        var desired = string.IsNullOrWhiteSpace(admission?.DesiredDirectionCode)
            ? "не обрано"
            : admission!.DesiredDirectionCode;
        var directions = admission is { Directions.Count: > 0 }
            ? string.Join("; ", admission.Directions.Select(d => $"{d.Code} — {d.Name} ({d.BranchOfKnowledge})"))
            : "немає";

        return
            $"Клас (рік навчання): {s.GradeLevel}. " +
            $"Бали НМТ: {nmt}. " +
            $"Бажаний напрям (код): {desired}. " +
            $"Доступні напрями вступу: {directions}. " +
            $"Середні бали за предметами: {Subjects(s)}. " +
            $"Середні бали за темами: {Topics(s)}.";
    }

    private AnalysisResult Parse(string content, AnalysisGoal goal)
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

        var defaultKind = DefaultKind(goal);
        var recommendations = new List<RecommendationFinding>();
        if (root.TryGetProperty("recommendations", out var recsElement) && recsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in recsElement.EnumerateArray())
            {
                // Only the profile-choice goal carries a structured cluster/profiles payload,
                // which feeds the profile-insight write-back and university demand analytics.
                ProfileCluster? cluster = null;
                IReadOnlyList<EducationProfile>? profiles = null;
                if (goal == AnalysisGoal.ProfileChoice)
                {
                    cluster = ProfileTaxonomy.TryParseCluster(GetString(element, "cluster", string.Empty));
                    profiles = GetStringArray(element, "profiles")
                        .Select(ProfileTaxonomy.TryParse)
                        .Where(p => p is not null)
                        .Select(p => p!.Value)
                        .ToList();
                }

                recommendations.Add(new RecommendationFinding(
                    ParseEnum(GetString(element, "kind", defaultKind.ToString()), defaultKind),
                    GetString(element, "title", "Рекомендація"),
                    GetString(element, "summary", string.Empty),
                    GetString(element, "rationale", string.Empty),
                    element.TryGetProperty("confidence", out var c) && c.TryGetDouble(out var confidence) ? confidence : 0.6,
                    cluster,
                    profiles));
            }
        }

        var summary = GetString(root, "summary", $"Модель {ModelName} ({goal}): {flags.Count} флагів, {recommendations.Count} рекомендацій.");
        return new AnalysisResult(ModelName, flags, recommendations, summary);
    }

    private static RecommendationKind DefaultKind(AnalysisGoal goal) => goal switch
    {
        AnalysisGoal.NmtFourthSubject => RecommendationKind.FourthSubjectChoice,
        AnalysisGoal.AdmissionDirection => RecommendationKind.AdmissionDirectionChoice,
        _ => RecommendationKind.ProfileChoice
    };

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
