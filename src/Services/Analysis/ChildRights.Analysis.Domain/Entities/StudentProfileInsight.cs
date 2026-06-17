using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// The latest profiling insight for a pupil, persisted so the platform can produce
/// <b>depersonalised</b> demand analytics for universities and communities. Aggregations
/// over these records never expose an individual pupil.
/// </summary>
public sealed class StudentProfileInsight : Entity
{
    private StudentProfileInsight()
    {
    }

    public StudentProfileInsight(
        Guid id,
        Guid studentId,
        Guid schoolId,
        ProfileCluster recommendedCluster,
        IEnumerable<EducationProfile> recommendedProfiles,
        ProfileCluster? desiredCluster,
        double confidence,
        IEnumerable<TopicStrength> topicStrengths,
        DateTime updatedUtc)
        : base(id)
    {
        StudentId = studentId;
        SchoolId = schoolId;
        Update(recommendedCluster, recommendedProfiles, desiredCluster, confidence, topicStrengths, updatedUtc);
    }

    public Guid StudentId { get; private set; }

    public Guid SchoolId { get; private set; }

    public ProfileCluster RecommendedCluster { get; private set; }

    public List<EducationProfile> RecommendedProfiles { get; private set; } = [];

    public ProfileCluster? DesiredCluster { get; private set; }

    public double Confidence { get; private set; }

    public List<TopicStrength> TopicStrengths { get; private set; } = [];

    public DateTime UpdatedUtc { get; private set; }

    public bool IsMismatch => DesiredCluster is not null && DesiredCluster != RecommendedCluster;

    public void Update(
        ProfileCluster recommendedCluster,
        IEnumerable<EducationProfile> recommendedProfiles,
        ProfileCluster? desiredCluster,
        double confidence,
        IEnumerable<TopicStrength> topicStrengths,
        DateTime updatedUtc)
    {
        RecommendedCluster = recommendedCluster;
        RecommendedProfiles = recommendedProfiles.ToList();
        DesiredCluster = desiredCluster;
        Confidence = confidence;
        TopicStrengths = topicStrengths.ToList();
        UpdatedUtc = updatedUtc;
    }
}

/// <summary>A pupil's strength in a single topic, stored for depersonalised demand analytics.</summary>
public sealed record TopicStrength(string Subject, string Topic, double Average);
