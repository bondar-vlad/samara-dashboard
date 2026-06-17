using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Entities;

namespace ChildRights.Analysis.Application.Admission;

internal static class AdmissionMapping
{
    public static AdmissionDirectionDto ToDto(AdmissionDirection direction) => new(
        direction.Code,
        direction.Name,
        direction.BranchOfKnowledge,
        direction.RelatedCluster.ToString(),
        ProfileTaxonomy.LocalizeCluster(direction.RelatedCluster),
        direction.NmtCoefficients.ToDictionary(kv => kv.Key.ToString(), kv => Math.Round(kv.Value, 3)),
        direction.KeySubjects,
        direction.KeyTopics,
        direction.Specialties.Select(s => new SpecialtyDto(s.Code, s.Name)).ToList());

    public static FourthSubjectScoreDto ToDto(FourthSubjectScore score) => new(
        score.Subject.ToString(),
        NmtSubjectCatalog.Localize(score.Subject),
        score.Score,
        score.EvidenceCount);

    public static DirectionMatchDto ToDto(DirectionMatch match) => new(
        match.DirectionCode,
        match.DirectionName,
        match.CompetitiveScore,
        match.TopicFit,
        match.CombinedScore);
}
