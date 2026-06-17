namespace ChildRights.Education.Application.Schools;

/// <summary>A profile offered by an institution, with localized names and its cluster.</summary>
public sealed record OfferedProfileDto(
    string Profile,
    string ProfileName,
    string Cluster,
    string ClusterName);

/// <summary>List item for the institutions catalogue.</summary>
public sealed record SchoolSummaryDto(
    Guid Id,
    string Name,
    string Community,
    string Region,
    string InstitutionType,
    string InstitutionTypeName,
    string Direction,
    int StudentCount,
    int OfferedProfileCount);

/// <summary>Full institution card: classes and the profiles it offers under the reform.</summary>
public sealed record SchoolDetailsDto(
    Guid Id,
    string Name,
    string Community,
    string Region,
    string InstitutionType,
    string InstitutionTypeName,
    string Direction,
    int StudentCount,
    IReadOnlyCollection<ClassDto> Classes,
    IReadOnlyCollection<OfferedProfileDto> OfferedProfiles);

public sealed record ClassDto(Guid Id, string Name, int GradeLevel, string ClassTeacher, int StudentCount);
