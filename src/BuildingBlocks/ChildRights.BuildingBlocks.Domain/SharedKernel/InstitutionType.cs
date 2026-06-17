namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// Types of education institutions in Ukraine that take part in the profile-education
/// reform network. Academic-direction institutions offer academic profiles; professional
/// ones offer professional profiles. The gymnasium is the basic-secondary feeder.
/// </summary>
public enum InstitutionType
{
    /// <summary>Гімназія — базова середня освіта (5–9). Feeds lyceums; offers no profiles itself.</summary>
    Gymnasium = 1,

    /// <summary>Академічний ліцей — профільна середня освіта, академічне спрямування (10–12).</summary>
    AcademicLyceum = 2,

    /// <summary>Професійний ліцей — профільна середня освіта, професійне спрямування.</summary>
    ProfessionalLyceum = 3,

    /// <summary>Фаховий коледж — фахова передвища освіта, професійне спрямування.</summary>
    ProfessionalCollege = 4,

    /// <summary>Науковий ліцей — академічне спрямування з дослідницьким фокусом (STEM).</summary>
    ScientificLyceum = 5,

    /// <summary>Мистецький ліцей — академічне спрямування зі спеціалізацією у мистецтві.</summary>
    ArtLyceum = 6,

    /// <summary>Спортивний ліцей — академічне спрямування зі спортивною спеціалізацією.</summary>
    SportsLyceum = 7,

    /// <summary>Військовий (військово-морський) ліцей / ліцей з посиленою військово-фізичною підготовкою.</summary>
    MilitaryLyceum = 8
}
