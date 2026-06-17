using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Education.Domain.Entities;

/// <summary>
/// A pupil — the aggregate root of the Education bounded context.
///
/// Profile-education reform model. A pupil enrols into one <b>cluster</b> and may choose
/// <b>several profiles within that cluster</b>. The record therefore carries:
///   • <see cref="DeclaredProfile"/>    — the specialisation currently followed (grade 10+);
///   • <see cref="DesiredProfiles"/>    — the profiles the pupil <i>wants</i> (self-reported, one cluster);
///   • <see cref="RecommendedProfiles"/>— the profiles the AI analysis recommends (one cluster).
/// A divergence between the desired and recommended <i>cluster</i> is a red-flag trigger.
/// </summary>
public sealed class Student : AggregateRoot
{
    private Student()
    {
    }

    public Student(
        Guid id,
        string fullName,
        DateOnly dateOfBirth,
        Guid schoolId,
        Guid classId,
        int gradeLevel,
        EducationProfile? declaredProfile = null,
        IEnumerable<EducationProfile>? desiredProfiles = null)
        : base(id)
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        SchoolId = schoolId;
        ClassId = classId;
        GradeLevel = gradeLevel;
        DeclaredProfile = declaredProfile;

        if (desiredProfiles is not null)
        {
            SetDesiredProfiles(desiredProfiles);
        }
    }

    public string FullName { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public Guid SchoolId { get; private set; }

    public Guid ClassId { get; private set; }

    public int GradeLevel { get; private set; }

    /// <summary>The specialisation the pupil currently follows (grade 10+), if any.</summary>
    public EducationProfile? DeclaredProfile { get; private set; }

    /// <summary>The profiles the pupil self-reports they want (all within one cluster).</summary>
    public IReadOnlyList<EducationProfile> DesiredProfiles { get; private set; } = [];

    /// <summary>The profiles recommended by the latest AI analysis (all within one cluster).</summary>
    public IReadOnlyList<EducationProfile> RecommendedProfiles { get; private set; } = [];

    /// <summary>Confidence (0..1) of the latest recommendation.</summary>
    public double? RecommendationConfidence { get; private set; }

    /// <summary>When the recommendation was last updated (UTC).</summary>
    public DateTime? RecommendationUpdatedUtc { get; private set; }

    /// <summary>The cluster implied by the desired profiles, if any.</summary>
    public ProfileCluster? DesiredCluster =>
        DesiredProfiles.Count == 0 ? null : ProfileTaxonomy.ClusterOf(DesiredProfiles[0]);

    /// <summary>The cluster implied by the recommended profiles, if any.</summary>
    public ProfileCluster? RecommendedCluster =>
        RecommendedProfiles.Count == 0 ? null : ProfileTaxonomy.ClusterOf(RecommendedProfiles[0]);

    public void DeclareProfile(EducationProfile profile) => DeclaredProfile = profile;

    /// <summary>
    /// Records the profiles the pupil wants. All must belong to the same cluster, reflecting
    /// the reform rule that a pupil chooses several profiles within a single cluster.
    /// </summary>
    public void SetDesiredProfiles(IEnumerable<EducationProfile> profiles)
    {
        var distinct = profiles.Distinct().ToList();
        if (distinct.Count == 0)
        {
            throw new ArgumentException("At least one desired profile is required.", nameof(profiles));
        }

        if (!ProfileTaxonomy.AreInSameCluster(distinct))
        {
            throw new InvalidOperationException("All desired profiles must belong to the same cluster.");
        }

        DesiredProfiles = distinct;
    }

    /// <summary>Applies a recommendation produced by the Analysis service (one cluster).</summary>
    public void ApplyRecommendation(IEnumerable<EducationProfile> profiles, double confidence, DateTime updatedUtc)
    {
        var distinct = profiles.Distinct().ToList();
        if (distinct.Count > 0 && !ProfileTaxonomy.AreInSameCluster(distinct))
        {
            throw new InvalidOperationException("All recommended profiles must belong to the same cluster.");
        }

        RecommendedProfiles = distinct;
        RecommendationConfidence = confidence;
        RecommendationUpdatedUtc = updatedUtc;
    }

    /// <summary>
    /// True when the pupil's desired cluster differs from the recommended one — the
    /// signal the platform uses to start a profile-orientation conversation.
    /// </summary>
    public bool HasProfileMismatch =>
        DesiredCluster is not null && RecommendedCluster is not null && DesiredCluster != RecommendedCluster;
}
