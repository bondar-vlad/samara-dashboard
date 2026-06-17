using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Rules;
using ChildRights.Analysis.Domain.Universities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Universities.Queries;

/// <summary>
/// Computes the gap between a pupil and one chosen programme (specialty): exactly which
/// subjects/topics to improve and by how much. Drives "I want this specialty — what should I do?".
/// </summary>
public sealed record GetStudentProgramGapQuery(Guid StudentId, Guid ProgramId) : IQuery<ProgramFitDto>;

internal sealed class GetStudentProgramGapQueryHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient) : IQueryHandler<GetStudentProgramGapQuery, ProgramFitDto>
{
    public async Task<Result<ProgramFitDto>> Handle(
        GetStudentProgramGapQuery query,
        CancellationToken cancellationToken)
    {
        var program = await context.UniversityPrograms
            .FirstOrDefaultAsync(p => p.Id == query.ProgramId, cancellationToken);

        if (program is null)
        {
            return Result.Failure<ProgramFitDto>(Error.NotFound($"Programme '{query.ProgramId}' was not found."));
        }

        var profile = await educationClient.GetStudentProfileAsync(query.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<ProgramFitDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found in the education service."));
        }

        var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
        var topicAverages = profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList();

        var fit = UniversityFitCalculator.Evaluate(program, subjectAverages, topicAverages);
        return Result.Success(UniversityProgramMapping.ToFitDto(program, fit));
    }
}
