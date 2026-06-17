namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// The recipient of an alert, recommendation or action. A single signal may
/// target several audiences at once (e.g. parent + class teacher + administration).
/// </summary>
public enum AudienceRole
{
    Student = 1,
    Parent = 2,
    Teacher = 3,
    ClassTeacher = 4,
    SchoolAdministration = 5,
    EducationSafetyOfficer = 6,
    SocialService = 7,
    JuvenilePolice = 8,
    MedicalService = 9,
    CommunityAuthority = 10,
    RegionalAuthority = 11
}
