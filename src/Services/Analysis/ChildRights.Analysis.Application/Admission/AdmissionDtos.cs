namespace ChildRights.Analysis.Application.Admission;

public sealed record SpecialtyDto(string Code, string Name);

/// <summary>An admission direction with its specialties and НМТ coefficients (catalogue view).</summary>
public sealed record AdmissionDirectionDto(
    string Code,
    string Name,
    string BranchOfKnowledge,
    string RelatedCluster,
    string RelatedClusterName,
    IReadOnlyDictionary<string, double> NmtCoefficients,
    IReadOnlyCollection<string> KeySubjects,
    IReadOnlyCollection<string> KeyTopics,
    IReadOnlyCollection<SpecialtyDto> Specialties);

public sealed record NmtSubjectDto(string Subject, string Name, bool Mandatory, bool FourthSubjectOption);

// ---- Widget 1: 4th НМТ subject ----

public sealed record FourthSubjectScoreDto(string Subject, string SubjectName, double Score, int EvidenceCount);

public sealed record FourthSubjectRecommendationDto(
    Guid StudentId,
    string? ChosenSubject,
    string? ChosenSubjectName,
    string? RecommendedSubject,
    string? RecommendedSubjectName,
    bool HasChoice,
    bool IsMatch,
    IReadOnlyCollection<FourthSubjectScoreDto> Ranked,
    string Rationale);

/// <summary>A pupil row in the "who chose this 4th subject" list (match / not-match).</summary>
public sealed record FourthSubjectStudentDto(
    Guid StudentId,
    string FullName,
    string ClassName,
    string? ChosenSubject,
    string? RecommendedSubject,
    bool IsMatch);

/// <summary>Distribution of 4th-subject choices (e.g. how many chose Mathematics-track options).</summary>
public sealed record FourthSubjectDistributionItemDto(string Subject, string SubjectName, int ChosenCount, int RecommendedCount);

// ---- Widget 2: admission direction ----

public sealed record DirectionMatchDto(
    string DirectionCode,
    string DirectionName,
    double? CompetitiveScore,
    double TopicFit,
    double CombinedScore);

public sealed record DirectionRecommendationDto(
    Guid StudentId,
    string? DesiredDirectionCode,
    string? DesiredDirectionName,
    string? RecommendedDirectionCode,
    string? RecommendedDirectionName,
    bool HasChoice,
    bool IsMatch,
    IReadOnlyDictionary<string, int> NmtScores,
    IReadOnlyCollection<DirectionMatchDto> Ranked,
    string Rationale);

/// <summary>A pupil row in the "who chose this direction" list (match / not-match).</summary>
public sealed record DirectionStudentDto(
    Guid StudentId,
    string FullName,
    string ClassName,
    string? DesiredDirectionCode,
    string? RecommendedDirectionCode,
    bool IsMatch);

/// <summary>Depersonalised distribution of direction choices (per-direction counts).</summary>
public sealed record DirectionDistributionItemDto(string DirectionCode, string DirectionName, int ChosenCount, int RecommendedCount);
