using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// A university programme (спеціальність) with its profile cluster and admission profile.
/// The key subjects and topics drive the topic-level student-fit and gap analysis.
/// </summary>
public sealed class UniversityProgram : Entity
{
    private UniversityProgram()
    {
    }

    public UniversityProgram(
        Guid id,
        Guid universityId,
        string universityName,
        string name,
        ProfileCluster cluster,
        IEnumerable<EducationProfile> relevantProfiles,
        IEnumerable<string> keySubjects,
        IEnumerable<string> keyTopics,
        double minCompetitiveAverage)
        : base(id)
    {
        UniversityId = universityId;
        UniversityName = universityName;
        Name = name;
        Cluster = cluster;
        RelevantProfiles = relevantProfiles.ToList();
        KeySubjects = keySubjects.ToList();
        KeyTopics = keyTopics.ToList();
        MinCompetitiveAverage = minCompetitiveAverage;
    }

    public Guid UniversityId { get; private set; }

    public string UniversityName { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public ProfileCluster Cluster { get; private set; }

    /// <summary>The reform profiles that best prepare a pupil for this programme.</summary>
    public List<EducationProfile> RelevantProfiles { get; private set; } = [];

    /// <summary>Subjects that matter most for admission and study success.</summary>
    public List<string> KeySubjects { get; private set; } = [];

    /// <summary>Specific topics that matter most (drives fine-grained gap analysis).</summary>
    public List<string> KeyTopics { get; private set; } = [];

    /// <summary>The competitive average (1–12) a candidate should reach in the key areas.</summary>
    public double MinCompetitiveAverage { get; private set; }
}
