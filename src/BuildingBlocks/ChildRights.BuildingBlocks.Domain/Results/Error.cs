namespace ChildRights.BuildingBlocks.Domain.Results;

/// <summary>
/// Represents a domain or application error in a structured, transport-agnostic way.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string description) => new("not_found", description);

    public static Error Validation(string description) => new("validation", description);

    public static Error Conflict(string description) => new("conflict", description);

    public static Error Unexpected(string description) => new("unexpected", description);
}
