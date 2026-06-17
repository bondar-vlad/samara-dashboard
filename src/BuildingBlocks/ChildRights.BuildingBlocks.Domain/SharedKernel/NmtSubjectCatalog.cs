namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// Canonical facts about НМТ subjects: which are mandatory, which are eligible as the 4th
/// (chosen) subject, Ukrainian display names, and the mapping from school subjects/topics
/// to the elective НМТ subject they best predict. Centralised so every service agrees.
/// </summary>
public static class NmtSubjectCatalog
{
    /// <summary>The three subjects every applicant must take.</summary>
    public static IReadOnlyList<NmtSubject> Mandatory { get; } =
    [
        NmtSubject.UkrainianLanguage,
        NmtSubject.Mathematics,
        NmtSubject.HistoryOfUkraine
    ];

    /// <summary>The subjects a pupil may choose as their 4th НМТ subject.</summary>
    public static IReadOnlyList<NmtSubject> FourthSubjectOptions { get; } =
    [
        NmtSubject.ForeignLanguage,
        NmtSubject.Biology,
        NmtSubject.Chemistry,
        NmtSubject.Physics,
        NmtSubject.Geography,
        NmtSubject.UkrainianLiterature
    ];

    public static IReadOnlyList<NmtSubject> All { get; } = Enum.GetValues<NmtSubject>();

    private static readonly Dictionary<NmtSubject, string> Names = new()
    {
        [NmtSubject.UkrainianLanguage] = "Українська мова",
        [NmtSubject.Mathematics] = "Математика",
        [NmtSubject.HistoryOfUkraine] = "Історія України",
        [NmtSubject.ForeignLanguage] = "Іноземна мова",
        [NmtSubject.Biology] = "Біологія",
        [NmtSubject.Chemistry] = "Хімія",
        [NmtSubject.Physics] = "Фізика",
        [NmtSubject.Geography] = "Географія",
        [NmtSubject.UkrainianLiterature] = "Українська література"
    };

    // School subject (and a few topic keywords) → the elective НМТ subject it best predicts.
    private static readonly Dictionary<string, NmtSubject> SchoolSubjectMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Біологія"] = NmtSubject.Biology,
        ["Хімія"] = NmtSubject.Chemistry,
        ["Фізика"] = NmtSubject.Physics,
        ["Географія"] = NmtSubject.Geography,
        ["Англійська мова"] = NmtSubject.ForeignLanguage,
        ["Іноземна мова"] = NmtSubject.ForeignLanguage,
        ["Німецька мова"] = NmtSubject.ForeignLanguage,
        ["Французька мова"] = NmtSubject.ForeignLanguage,
        ["Українська література"] = NmtSubject.UkrainianLiterature,
        ["Зарубіжна література"] = NmtSubject.UkrainianLiterature
    };

    public static bool IsMandatory(NmtSubject subject) => Mandatory.Contains(subject);

    public static bool IsFourthSubjectOption(NmtSubject subject) => FourthSubjectOptions.Contains(subject);

    public static string Localize(NmtSubject subject) => Names[subject];

    /// <summary>Maps a school subject name to the elective НМТ subject it best predicts, if any.</summary>
    public static NmtSubject? FromSchoolSubject(string schoolSubject) =>
        SchoolSubjectMap.TryGetValue(schoolSubject.Trim(), out var subject) ? subject : null;

    public static NmtSubject? TryParse(string? value) =>
        Enum.TryParse<NmtSubject>(value, ignoreCase: true, out var parsed) ? parsed : null;
}
