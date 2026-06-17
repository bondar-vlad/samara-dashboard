using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Admission.Queries;

/// <summary>Lists admission directions (with specialties + НМТ coefficients) — the reference catalogue.</summary>
public sealed record ListAdmissionDirectionsQuery(string? Cluster = null)
    : IQuery<IReadOnlyCollection<AdmissionDirectionDto>>;

internal sealed class ListAdmissionDirectionsQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<ListAdmissionDirectionsQuery, IReadOnlyCollection<AdmissionDirectionDto>>
{
    public async Task<Result<IReadOnlyCollection<AdmissionDirectionDto>>> Handle(
        ListAdmissionDirectionsQuery query,
        CancellationToken cancellationToken)
    {
        var directions = await context.AdmissionDirections
            .Include(d => d.Specialties)
            .OrderBy(d => d.Code)
            .ToListAsync(cancellationToken);

        if (ProfileTaxonomy.TryParseCluster(query.Cluster) is { } cluster)
        {
            directions = directions.Where(d => d.RelatedCluster == cluster).ToList();
        }

        IReadOnlyCollection<AdmissionDirectionDto> result = directions.Select(AdmissionMapping.ToDto).ToList();
        return Result.Success(result);
    }
}

/// <summary>Lists the НМТ subjects (mandatory + 4th-subject options) for client dropdowns.</summary>
public sealed record ListNmtSubjectsQuery : IQuery<IReadOnlyCollection<NmtSubjectDto>>;

internal sealed class ListNmtSubjectsQueryHandler
    : IQueryHandler<ListNmtSubjectsQuery, IReadOnlyCollection<NmtSubjectDto>>
{
    public Task<Result<IReadOnlyCollection<NmtSubjectDto>>> Handle(
        ListNmtSubjectsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<NmtSubjectDto> result = NmtSubjectCatalog.All
            .Select(s => new NmtSubjectDto(
                s.ToString(),
                NmtSubjectCatalog.Localize(s),
                NmtSubjectCatalog.IsMandatory(s),
                NmtSubjectCatalog.IsFourthSubjectOption(s)))
            .ToList();

        return Task.FromResult(Result.Success(result));
    }
}
