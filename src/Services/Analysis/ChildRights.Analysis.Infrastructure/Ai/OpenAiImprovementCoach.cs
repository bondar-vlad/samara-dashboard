using System.Net.Http.Json;
using System.Text.Json;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChildRights.Analysis.Infrastructure.Ai;

/// <summary>
/// LLM-backed improvement coach. Given the measured gaps toward a pupil's <b>chosen</b> profile
/// or admission direction, it produces concrete, encouraging study advice ("what to pull up in
/// which subjects and topics"). Registered only when an OpenAI key is configured; otherwise the
/// <see cref="UnavailableImprovementCoach"/> reports the model as not connected. Reuses the same
/// resilient HTTP client as the analysis provider and sends no personal identifiers.
/// </summary>
internal sealed class OpenAiImprovementCoach(
    IHttpClientFactory httpClientFactory,
    IOptions<AiOptions> options,
    ILogger<OpenAiImprovementCoach> logger) : IImprovementCoach
{
    private const string SystemPrompt =
        "Ти — освітній коуч-радник для українського школяра. Учень уже ОБРАВ ціль (профіль навчання " +
        "або напрям вступу), проте дані вказують радше на іншу. Підтримай вибір учня та склади КОНКРЕТНИЙ " +
        "план, ЩО ПІДТЯГНУТИ у вказаних предметах і темах, щоб обрана ціль стала реалістичною. Для кожної " +
        "області з прогалиною дай практичну заохочувальну пораду (що саме вивчати й як працювати). " +
        "Поверни ВИКЛЮЧНО JSON: {\"summary\":string,\"items\":[{\"name\":string,\"advice\":string}]," +
        "\"steps\":[string]}. Текст — українською, без персональних даних.";

    public bool IsAvailable => true;

    public string ModelName => options.Value.OpenAi.Model;

    public async Task<ImprovementCoachResult> CoachAsync(
        ImprovementCoachRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(OpenAiAnalysisProvider.HttpClientName);

            var payload = new
            {
                model = options.Value.OpenAi.Model,
                temperature = 0.3,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
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

            return Parse(content ?? string.Empty);
        }
        catch (Exception ex)
        {
            // Surfaced to the caller as "model not connected"; logged for diagnostics.
            logger.LogWarning(ex, "Improvement-plan LLM call failed; reporting the model as not connected.");
            throw;
        }
    }

    private static string BuildUserPrompt(ImprovementCoachRequest r)
    {
        var targetKind = r.TargetKind == "direction" ? "напрям вступу" : "профіль навчання";

        var gaps = r.Gaps.Count > 0
            ? string.Join("; ", r.Gaps.Select(g =>
                $"{(g.Area == "topic" ? "тема" : "предмет")} «{g.Name}» — поточний рівень {g.Current:0.0}/12, ціль {g.Target:0.0}"))
            : "явних прогалин не виявлено";

        var strengths = r.Strengths.Count > 0 ? string.Join(", ", r.Strengths) : "—";

        return
            $"Клас (рік навчання): {r.GradeLevel}. " +
            $"Обрана ціль ({targetKind}): {r.TargetName}. " +
            $"Сильні предмети учня: {strengths}. " +
            $"Прогалини до цілі: {gaps}. " +
            "Сформуй план підтягування саме під цю обрану ціль.";
    }

    private static ImprovementCoachResult Parse(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var items = new List<ImprovementAdvice>();
        if (root.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in itemsElement.EnumerateArray())
            {
                var name = GetString(element, "name");
                if (name.Length > 0)
                {
                    items.Add(new ImprovementAdvice(name, GetString(element, "advice")));
                }
            }
        }

        var steps = new List<string>();
        if (root.TryGetProperty("steps", out var stepsElement) && stepsElement.ValueKind == JsonValueKind.Array)
        {
            steps.AddRange(stepsElement.EnumerateArray()
                .Select(e => e.GetString() ?? string.Empty)
                .Where(s => s.Length > 0));
        }

        return new ImprovementCoachResult(GetString(root, "summary"), items, steps);
    }

    private static string GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
}
