using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Rules;

/// <summary>
/// Maps school subjects (Ukrainian names) to specialisation areas, and aggregates
/// per-subject averages into per-area averages used by the profiling engine.
/// </summary>
public static class SubjectProfileMap
{
    private static readonly Dictionary<string, ProfileArea> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Математика"] = ProfileArea.Mathematics,
        ["Алгебра"] = ProfileArea.Mathematics,
        ["Геометрія"] = ProfileArea.Mathematics,
        ["Фізика"] = ProfileArea.NaturalSciences,
        ["Біологія"] = ProfileArea.NaturalSciences,
        ["Хімія"] = ProfileArea.NaturalSciences,
        ["Географія"] = ProfileArea.NaturalSciences,
        ["Українська мова"] = ProfileArea.Philology,
        ["Українська література"] = ProfileArea.Philology,
        ["Англійська мова"] = ProfileArea.Philology,
        ["Зарубіжна література"] = ProfileArea.Philology,
        ["Історія"] = ProfileArea.SocialSciences,
        ["Правознавство"] = ProfileArea.SocialSciences,
        ["Громадянська освіта"] = ProfileArea.SocialSciences,
        ["Інформатика"] = ProfileArea.Technology,
        ["Технології"] = ProfileArea.Technology,
        ["Трудове навчання"] = ProfileArea.Technology,
        ["Мистецтво"] = ProfileArea.Arts,
        ["Музичне мистецтво"] = ProfileArea.Arts,
        ["Образотворче мистецтво"] = ProfileArea.Arts
    };

    public static ProfileArea? Resolve(string subject) =>
        Map.TryGetValue(subject.Trim(), out var area) ? area : null;

    public static IReadOnlyDictionary<ProfileArea, double> AreaAverages(
        IReadOnlyDictionary<string, double> subjectAverages) =>
        subjectAverages
            .Select(kv => (Area: Resolve(kv.Key), kv.Value))
            .Where(x => x.Area is not null)
            .GroupBy(x => x.Area!.Value)
            .ToDictionary(g => g.Key, g => g.Average(x => x.Value));

    public static string Localize(ProfileArea area) => area switch
    {
        ProfileArea.Mathematics => "Математичний",
        ProfileArea.NaturalSciences => "Природничий",
        ProfileArea.Philology => "Філологічний",
        ProfileArea.SocialSciences => "Суспільно-гуманітарний",
        ProfileArea.Technology => "Технологічний",
        ProfileArea.Arts => "Мистецький",
        _ => area.ToString()
    };

    public static ProfileArea? ParseDeclared(string declaredProfile) => declaredProfile switch
    {
        "Mathematics" => ProfileArea.Mathematics,
        "NaturalSciences" => ProfileArea.NaturalSciences,
        "Philology" => ProfileArea.Philology,
        "SocialSciences" => ProfileArea.SocialSciences,
        "Technology" => ProfileArea.Technology,
        "Arts" => ProfileArea.Arts,
        _ => null
    };
}
