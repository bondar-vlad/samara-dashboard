using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// An admission direction (напрям вступу) — a national grouping that aggregates one or more
/// <see cref="Specialty"/> (спеціальності) offered across many universities. Each direction
/// weights the НМТ subjects with its own competition coefficients, and declares the school
/// subjects/topics that best predict success in it.
/// </summary>
public sealed class AdmissionDirection : AggregateRoot
{
    private readonly List<Specialty> _specialties = [];

    private AdmissionDirection()
    {
    }

    public AdmissionDirection(
        Guid id,
        string code,
        string name,
        string branchOfKnowledge,
        ProfileCluster relatedCluster,
        IReadOnlyDictionary<NmtSubject, double> nmtCoefficients,
        IEnumerable<string> keySubjects,
        IEnumerable<string> keyTopics)
        : base(id)
    {
        Code = code;
        Name = name;
        BranchOfKnowledge = branchOfKnowledge;
        RelatedCluster = relatedCluster;
        NmtCoefficients = Normalize(nmtCoefficients);
        KeySubjects = keySubjects.ToList();
        KeyTopics = keyTopics.ToList();
    }

    /// <summary>National code of the direction (e.g. "12" for ІТ branch grouping).</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>Галузь знань the direction belongs to.</summary>
    public string BranchOfKnowledge { get; private set; } = string.Empty;

    /// <summary>The reform profile cluster this direction is most aligned with.</summary>
    public ProfileCluster RelatedCluster { get; private set; }

    /// <summary>Competition coefficients per НМТ subject; normalised so the weights sum to 1.</summary>
    public Dictionary<NmtSubject, double> NmtCoefficients { get; private set; } = [];

    /// <summary>School subjects that best predict success in this direction.</summary>
    public List<string> KeySubjects { get; private set; } = [];

    /// <summary>Curriculum topics that best predict success in this direction.</summary>
    public List<string> KeyTopics { get; private set; } = [];

    public IReadOnlyCollection<Specialty> Specialties => _specialties.AsReadOnly();

    public void AddSpecialty(string code, string name)
    {
        if (_specialties.All(s => s.Code != code))
        {
            _specialties.Add(new Specialty(Guid.NewGuid(), Id, code, name));
        }
    }

    // Defensive normalisation: coefficients are stored summing to 1 so the competitive score
    // lands on the same 100–200 scale as the НМТ inputs regardless of how they were supplied.
    private static Dictionary<NmtSubject, double> Normalize(IReadOnlyDictionary<NmtSubject, double> coefficients)
    {
        var total = coefficients.Values.Sum();
        if (total <= 0)
        {
            return coefficients.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        return coefficients.ToDictionary(kv => kv.Key, kv => kv.Value / total);
    }
}
