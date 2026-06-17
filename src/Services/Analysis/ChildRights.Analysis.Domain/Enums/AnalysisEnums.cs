namespace ChildRights.Analysis.Domain.Enums;

/// <summary>The kind of recommendation produced by the profiling engine.</summary>
public enum RecommendationKind
{
    /// <summary>Grade 9 pupil: which specialisation to choose for grade 10.</summary>
    ProfileChoice = 1,

    /// <summary>Grade 10+ pupil: switch to a better-fitting specialisation.</summary>
    ProfileChange = 2,

    /// <summary>School/community: open a new specialisation profile.</summary>
    OpenProfile = 3,

    /// <summary>School: close an under-subscribed specialisation profile.</summary>
    CloseProfile = 4,

    /// <summary>Community/region: create a course for a continuing-education academy.</summary>
    AcademyCourse = 5
}

/// <summary>Lifecycle of a red flag.</summary>
public enum FlagStatus
{
    Open = 0,
    Acknowledged = 1,
    Resolved = 2
}

/// <summary>Lifecycle of an analysis run.</summary>
public enum RunStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}
