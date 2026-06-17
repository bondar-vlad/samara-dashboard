namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// Subjects of the Ukrainian National Multi-subject Test (НМТ). Three are mandatory for
/// every applicant; the rest are the options a pupil picks the <b>4th subject</b> from.
/// </summary>
public enum NmtSubject
{
    // Обовʼязкові
    /// <summary>Українська мова (обовʼязковий).</summary>
    UkrainianLanguage = 1,

    /// <summary>Математика (обовʼязковий).</summary>
    Mathematics = 2,

    /// <summary>Історія України (обовʼязковий).</summary>
    HistoryOfUkraine = 3,

    // Предмети на вибір (4-й предмет)
    /// <summary>Іноземна мова.</summary>
    ForeignLanguage = 10,

    /// <summary>Біологія.</summary>
    Biology = 11,

    /// <summary>Хімія.</summary>
    Chemistry = 12,

    /// <summary>Фізика.</summary>
    Physics = 13,

    /// <summary>Географія.</summary>
    Geography = 14,

    /// <summary>Українська література.</summary>
    UkrainianLiterature = 15
}
