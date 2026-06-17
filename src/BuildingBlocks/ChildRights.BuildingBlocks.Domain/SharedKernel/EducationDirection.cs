namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// The "спрямування" (direction) of specialised secondary education introduced by the
/// 2027 reform. An institution provides one of these; profiles are offered per direction.
/// </summary>
public enum EducationDirection
{
    /// <summary>Базова середня освіта (gymnasium, grades 5–9). No specialised profiles yet.</summary>
    Basic = 0,

    /// <summary>Академічне спрямування — academic lyceums, prepares for higher education.</summary>
    Academic = 1,

    /// <summary>Професійне спрямування — professional lyceums and colleges, secondary education plus a profession.</summary>
    Professional = 2
}
