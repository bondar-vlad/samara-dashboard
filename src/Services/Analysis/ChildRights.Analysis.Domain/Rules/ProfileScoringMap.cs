using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Domain.Rules;

/// <summary>
/// Maps school subjects <b>and curriculum topics</b> onto reform <see cref="EducationProfile"/>s
/// and scores each profile from a pupil's grades. Topic-level signals are weighted more
/// heavily than whole-subject averages, so (for example) a strong "Фінансове право" topic
/// inside law steers the pupil toward the business cluster rather than the broad legal one.
/// Pure and deterministic — the testable heart of the profiling engine.
/// </summary>
public static class ProfileScoringMap
{
    // Whole-subject contributions (subject name → profiles it feeds).
    private static readonly Dictionary<string, EducationProfile[]> SubjectMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Математика"] = [EducationProfile.NaturalMathematical, EducationProfile.InformationTechnology, EducationProfile.EngineeringTechnological],
        ["Алгебра"] = [EducationProfile.NaturalMathematical, EducationProfile.InformationTechnology],
        ["Геометрія"] = [EducationProfile.NaturalMathematical, EducationProfile.Construction],
        ["Фізика"] = [EducationProfile.NaturalMathematical, EducationProfile.EngineeringTechnological, EducationProfile.Construction, EducationProfile.TransportLogistics],
        ["Біологія"] = [EducationProfile.NaturalMathematical, EducationProfile.Medical, EducationProfile.Agricultural],
        ["Хімія"] = [EducationProfile.NaturalMathematical, EducationProfile.Medical, EducationProfile.Agricultural, EducationProfile.BeautyServicesDesign],
        ["Географія"] = [EducationProfile.NaturalMathematical, EducationProfile.HospitalityEvents],
        ["Інформатика"] = [EducationProfile.InformationTechnology, EducationProfile.EngineeringTechnological],
        ["Технології"] = [EducationProfile.EngineeringTechnological, EducationProfile.Construction, EducationProfile.Agricultural],
        ["Трудове навчання"] = [EducationProfile.Construction, EducationProfile.BeautyServicesDesign],
        ["Економіка"] = [EducationProfile.BusinessAdministration, EducationProfile.SocialHumanitarian],
        ["Правознавство"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Історія"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Громадянська освіта"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Українська мова"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Українська література"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Англійська мова"] = [EducationProfile.EducationalHumanitarian, EducationProfile.BusinessAdministration, EducationProfile.HospitalityEvents],
        ["Іноземна мова"] = [EducationProfile.EducationalHumanitarian, EducationProfile.BusinessAdministration, EducationProfile.HospitalityEvents],
        ["Зарубіжна література"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Мистецтво"] = [EducationProfile.BeautyServicesDesign],
        ["Музичне мистецтво"] = [EducationProfile.BeautyServicesDesign],
        ["Образотворче мистецтво"] = [EducationProfile.BeautyServicesDesign]
    };

    // Topic-level contributions (curriculum topic → profiles). These dominate the score
    // because they capture the pupil's specific interests within a subject.
    private static readonly Dictionary<string, EducationProfile[]> TopicMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Фінансове право"] = [EducationProfile.BusinessAdministration],
        ["Господарське право"] = [EducationProfile.BusinessAdministration],
        ["Податкове право"] = [EducationProfile.BusinessAdministration],
        ["Конституційне право"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Кримінальне право"] = [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
        ["Фінанси та інвестиції"] = [EducationProfile.BusinessAdministration],
        ["Мікроекономіка"] = [EducationProfile.BusinessAdministration],
        ["Макроекономіка"] = [EducationProfile.BusinessAdministration],
        ["Підприємництво"] = [EducationProfile.BusinessAdministration],
        ["Програмування"] = [EducationProfile.InformationTechnology],
        ["Алгоритми"] = [EducationProfile.InformationTechnology],
        ["Бази даних"] = [EducationProfile.InformationTechnology],
        ["Робототехніка"] = [EducationProfile.EngineeringTechnological],
        ["Генетика"] = [EducationProfile.Medical],
        ["Анатомія людини"] = [EducationProfile.Medical],
        ["Мікробіологія"] = [EducationProfile.Medical],
        ["Органічна хімія"] = [EducationProfile.Medical, EducationProfile.Agricultural],
        ["Агрономія"] = [EducationProfile.Agricultural],
        ["Логістика"] = [EducationProfile.TransportLogistics],
        ["Будівельні конструкції"] = [EducationProfile.Construction],
        ["Креслення"] = [EducationProfile.Construction, EducationProfile.EngineeringTechnological],
        ["Менеджмент подій"] = [EducationProfile.HospitalityEvents],
        ["Комунікації"] = [EducationProfile.HospitalityEvents, EducationProfile.BusinessAdministration],
        ["Дизайн"] = [EducationProfile.BeautyServicesDesign],
        ["Маркетинг"] = [EducationProfile.BusinessAdministration, EducationProfile.BeautyServicesDesign]
    };

    private const double SubjectWeight = 1.0;
    private const double TopicWeight = 2.0;

    /// <summary>A single grade signal: its value, weight and the profiles it feeds.</summary>
    private readonly record struct Signal(double Value, double Weight, EducationProfile[] Profiles);

    private static List<Signal> BuildSignals(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        var signals = new List<Signal>();

        foreach (var (subject, average) in subjectAverages)
        {
            if (SubjectMap.TryGetValue(subject.Trim(), out var profiles))
            {
                signals.Add(new Signal(average, SubjectWeight, profiles));
            }
        }

        foreach (var topic in topicAverages)
        {
            if (TopicMap.TryGetValue(topic.Topic.Trim(), out var profiles))
            {
                signals.Add(new Signal(topic.Average, TopicWeight, profiles));
            }
            else if (SubjectMap.TryGetValue(topic.Subject.Trim(), out var subjectProfiles))
            {
                signals.Add(new Signal(topic.Average, SubjectWeight, subjectProfiles));
            }
        }

        return signals;
    }

    /// <summary>
    /// Computes a 0–12 score per profile from subject averages and topic averages.
    /// Returns only profiles for which there is at least some evidence.
    /// </summary>
    public static IReadOnlyDictionary<EducationProfile, double> ScoreProfiles(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        var weighted = new Dictionary<EducationProfile, (double Sum, double Weight)>();

        foreach (var signal in BuildSignals(subjectAverages, topicAverages))
        {
            foreach (var profile in signal.Profiles)
            {
                var current = weighted.GetValueOrDefault(profile);
                weighted[profile] = (current.Sum + (signal.Value * signal.Weight), current.Weight + signal.Weight);
            }
        }

        return weighted.ToDictionary(kv => kv.Key, kv => kv.Value.Sum / kv.Value.Weight);
    }

    /// <summary>
    /// Computes a 0–12 score per cluster. Each grade signal counts at most once per cluster,
    /// and the weighted average is shrunk toward zero when evidence is thin (Bayesian shrinkage),
    /// so a cluster's score reflects the <b>breadth and strength</b> of evidence — a single high
    /// signal cannot outrank a well-evidenced cluster.
    /// </summary>
    public static IReadOnlyDictionary<ProfileCluster, double> ScoreClusters(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        // Pseudo-evidence (in weight units) that pulls thin clusters toward zero.
        const double EvidencePrior = 2.0;

        var weighted = new Dictionary<ProfileCluster, (double Sum, double Weight)>();

        foreach (var signal in BuildSignals(subjectAverages, topicAverages))
        {
            foreach (var cluster in signal.Profiles.Select(ProfileTaxonomy.ClusterOf).Distinct())
            {
                var current = weighted.GetValueOrDefault(cluster);
                weighted[cluster] = (current.Sum + (signal.Value * signal.Weight), current.Weight + signal.Weight);
            }
        }

        return weighted.ToDictionary(
            kv => kv.Key,
            kv =>
            {
                var average = kv.Value.Sum / kv.Value.Weight;
                var evidenceFactor = kv.Value.Weight / (kv.Value.Weight + EvidencePrior);
                return average * evidenceFactor;
            });
    }

    /// <summary>
    /// The school subjects and curriculum topics that feed a given cluster — i.e. the areas a
    /// pupil aiming for that cluster needs to be strong in. Derived by inverting the subject/topic
    /// maps. Used to build a concrete "what to pull up" gap analysis toward a chosen profile.
    /// </summary>
    public static (IReadOnlyList<string> Subjects, IReadOnlyList<string> Topics) KeySignalsForCluster(
        ProfileCluster cluster)
    {
        var subjects = SubjectMap
            .Where(kv => kv.Value.Any(p => ProfileTaxonomy.ClusterOf(p) == cluster))
            .Select(kv => kv.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var topics = TopicMap
            .Where(kv => kv.Value.Any(p => ProfileTaxonomy.ClusterOf(p) == cluster))
            .Select(kv => kv.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return (subjects, topics);
    }
}
