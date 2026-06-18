namespace ChildRights.Analysis.Infrastructure.Configuration;

/// <summary>
/// AI configuration (bound from the "Ai" section). Lets operators swap the default
/// model and enable an LLM provider without code changes.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>Model used when a request does not specify one. Defaults to the deterministic engine.</summary>
    public string DefaultModel { get; set; } = "rule-based-v1";

    /// <summary>Scheduled-sweep interval in minutes. 0 disables the scheduler.</summary>
    public int ScheduleMinutes { get; set; }

    /// <summary>Optional school to target during scheduled sweeps; null = all pupils.</summary>
    public Guid? ScheduledSchoolId { get; set; }

    /// <summary>
    /// When true (default), the service runs a one-time analysis sweep of every pupil at startup,
    /// so risks (red flags) and recommendations are detected <b>automatically</b> as soon as the
    /// data is available — without waiting for an incoming event or a manual run. It is guarded by
    /// the existing analysis runs, so it executes only once on a fresh database.
    /// </summary>
    public bool BootstrapAnalysisOnStartup { get; set; } = true;

    public OpenAiOptions OpenAi { get; set; } = new();
}

public sealed class OpenAiOptions
{
    public string? ApiKey { get; set; }

    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>The LLM provider is only registered when an API key is supplied.</summary>
    public bool Enabled => !string.IsNullOrWhiteSpace(ApiKey);
}
