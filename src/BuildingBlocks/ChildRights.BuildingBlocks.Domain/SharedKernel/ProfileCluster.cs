namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// An education cluster (освітній кластер) of the 2027 reform. Clusters sit between the
/// <see cref="EducationDirection"/> (academic/professional) and individual
/// <see cref="EducationProfile"/>s. A pupil enrols into one cluster and may select
/// <b>several profiles within that cluster</b>.
///
/// Values are banded by direction: 1–9 academic, 10+ professional.
/// </summary>
public enum ProfileCluster
{
    // Академічне спрямування
    /// <summary>Природничо-математичний (STEM).</summary>
    AcademicNaturalMathematical = 1,

    /// <summary>Суспільно-гуманітарний.</summary>
    AcademicSocialHumanitarian = 2,

    // Професійне спрямування
    /// <summary>Технічний та інженерний (будівництво, інженерія, транспорт-логістика).</summary>
    ProfessionalTechnical = 10,

    /// <summary>Інформаційні технології.</summary>
    ProfessionalInformationTechnology = 11,

    /// <summary>Природничо-аграрний та медичний.</summary>
    ProfessionalLifeSciences = 12,

    /// <summary>Бізнес та сфера послуг.</summary>
    ProfessionalBusinessServices = 13,

    /// <summary>Освітньо-гуманітарний (професійний).</summary>
    ProfessionalHumanitarian = 14
}
