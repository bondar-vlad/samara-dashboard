namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// The canonical catalogue of specialisation profiles for the 2027 Ukrainian
/// profile-education reform (МОН типова освітня програма). A shared national standard,
/// so it lives in the shared kernel and is used by both Education and Analysis.
///
/// Two academic clusters (values 1–2) plus the ten professional directions
/// (професійні напрями, values 10–19).
/// </summary>
public enum EducationProfile
{
    // Академічні профілі (academic clusters)
    /// <summary>Природничо-математичний (STEM): фізика, хімія, біологія, географія, математика.</summary>
    NaturalMathematical = 1,

    /// <summary>Суспільно-гуманітарний: історія, право, іноземні мови.</summary>
    SocialHumanitarian = 2,

    // Професійні напрями (10 professional directions)
    /// <summary>Аграрний: біологія, хімія, технології вирощування та управління земельними ресурсами.</summary>
    Agricultural = 10,

    /// <summary>Будівельний: поглиблена математика, фізика, креслення, технології будівництва.</summary>
    Construction = 11,

    /// <summary>Транспортно-логістичний: фізика, математика, техніка та планування потоків.</summary>
    TransportLogistics = 12,

    /// <summary>Інженерно-технологічний: фізика, математика, інформатика, робототехніка.</summary>
    EngineeringTechnological = 13,

    /// <summary>Медичний: посилені біологія, хімія та основи здоров’я.</summary>
    Medical = 14,

    /// <summary>ІТ: математика, алгоритми, програмування, цифрові технології.</summary>
    InformationTechnology = 15,

    /// <summary>Бізнес та адміністрування: математика, економіка, іноземні мови.</summary>
    BusinessAdministration = 16,

    /// <summary>Освітньо-гуманітарний: мови, література, історія, правознавство, психологія.</summary>
    EducationalHumanitarian = 17,

    /// <summary>Гостинність та організація подій: іноземні мови, географія, етика, комунікації, менеджмент.</summary>
    HospitalityEvents = 18,

    /// <summary>Послуги краси та дизайн: мистецтво, хімія, основи маркетингу, технології іміджу.</summary>
    BeautyServicesDesign = 19
}
