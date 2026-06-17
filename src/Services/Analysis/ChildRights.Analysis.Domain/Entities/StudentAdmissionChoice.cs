using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// A pupil's self-reported admission inputs: their НМТ scores, the 4th subject they chose,
/// and the admission direction they want. Stored in Analysis so the platform can compare the
/// pupil's choices against the data-driven recommendation and list pupils per choice (match /
/// not-match), without changing the Education service.
/// </summary>
public sealed class StudentAdmissionChoice : AggregateRoot
{
    private StudentAdmissionChoice()
    {
    }

    public StudentAdmissionChoice(Guid id, Guid studentId, Guid schoolId)
        : base(id)
    {
        StudentId = studentId;
        SchoolId = schoolId;
    }

    public Guid StudentId { get; private set; }

    public Guid SchoolId { get; private set; }

    /// <summary>The 4th НМТ subject the pupil chose, if any.</summary>
    public NmtSubject? ChosenFourthSubject { get; private set; }

    /// <summary>The admission direction code the pupil wants, if any.</summary>
    public string? DesiredDirectionCode { get; private set; }

    /// <summary>НМТ scores per subject (100–200 scale).</summary>
    public Dictionary<NmtSubject, int> NmtScores { get; private set; } = [];

    public DateTime UpdatedUtc { get; private set; }

    public void SetFourthSubject(NmtSubject subject, DateTime utcNow)
    {
        if (!NmtSubjectCatalog.IsFourthSubjectOption(subject))
        {
            throw new InvalidOperationException($"'{subject}' is not a valid 4th НМТ subject option.");
        }

        ChosenFourthSubject = subject;
        UpdatedUtc = utcNow;
    }

    public void SetDesiredDirection(string directionCode, DateTime utcNow)
    {
        DesiredDirectionCode = directionCode;
        UpdatedUtc = utcNow;
    }

    public void SetNmtScores(IReadOnlyDictionary<NmtSubject, int> scores, DateTime utcNow)
    {
        NmtScores = scores.ToDictionary(kv => kv.Key, kv => kv.Value);
        UpdatedUtc = utcNow;
    }
}
