namespace ChildRights.Analysis.Application.Universities;

public sealed record UniversityDto(Guid Id, string Name, string City, string Region, int ProgramCount);

public sealed record UniversityProgramDto(
    Guid Id,
    Guid UniversityId,
    string UniversityName,
    string Name,
    string Cluster,
    string ClusterName,
    IReadOnlyCollection<string> RelevantProfiles,
    IReadOnlyCollection<string> KeySubjects,
    IReadOnlyCollection<string> KeyTopics,
    double MinCompetitiveAverage);

/// <summary>An area (subject or topic) the pupil should improve to be competitive.</summary>
public sealed record AdmissionGapDto(string Area, string Name, double Current, double Target, double Gap);

/// <summary>How well a pupil matches a programme, with concrete improvement guidance.</summary>
public sealed record ProgramFitDto(
    Guid ProgramId,
    Guid UniversityId,
    string UniversityName,
    string ProgramName,
    string Cluster,
    string ClusterName,
    double FitScore,
    bool MeetsThreshold,
    IReadOnlyCollection<string> Strengths,
    IReadOnlyCollection<AdmissionGapDto> Gaps,
    IReadOnlyCollection<string> Advice);

/// <summary>The pupil's ranked university-programme matches.</summary>
public sealed record StudentUniversityFitDto(
    Guid StudentId,
    string RecommendedCluster,
    string RecommendedClusterName,
    IReadOnlyCollection<ProgramFitDto> Matches);

/// <summary>Depersonalised demand for a single programme/specialty.</summary>
public sealed record ProgramDemandDto(
    Guid ProgramId,
    Guid UniversityId,
    string UniversityName,
    string ProgramName,
    string Cluster,
    string ClusterName,
    int ExplicitInterest,
    int DataDrivenCandidates,
    int TotalDemand);

/// <summary>Depersonalised demand report for a university (or the whole catalogue).</summary>
public sealed record UniversityDemandDto(
    Guid? UniversityId,
    int StudentsConsidered,
    IReadOnlyCollection<ProgramDemandDto> Programs);
