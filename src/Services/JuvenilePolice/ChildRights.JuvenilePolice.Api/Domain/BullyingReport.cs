using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.JuvenilePolice.Api.Domain;

/// <summary>A bullying report filed for a class — a class-level child-safety signal.</summary>
public sealed class BullyingReport : Entity
{
    private BullyingReport()
    {
    }

    public BullyingReport(Guid id, Guid classId, Guid schoolId, FlagSeverity severity, string summary, DateOnly filedOn)
        : base(id)
    {
        ClassId = classId;
        SchoolId = schoolId;
        Severity = severity;
        Summary = summary;
        FiledOn = filedOn;
    }

    public Guid ClassId { get; private set; }

    public Guid SchoolId { get; private set; }

    public FlagSeverity Severity { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public DateOnly FiledOn { get; private set; }
}
