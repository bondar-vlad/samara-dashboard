using System.Net;
using System.Net.Http.Json;
using ChildRights.Analysis.Application.Abstractions;

namespace ChildRights.Analysis.Infrastructure.Education;

/// <summary>Resilient typed HTTP client that reads pupil data from the Education service.</summary>
internal sealed class EducationDataClient(HttpClient httpClient) : IEducationDataClient
{
    public async Task<IReadOnlyList<EducationStudentRef>> GetStudentsAsync(
        Guid? schoolId,
        CancellationToken cancellationToken = default)
    {
        var url = schoolId is { } id ? $"api/students?schoolId={id}" : "api/students";
        var students = await httpClient.GetFromJsonAsync<List<EducationStudentRef>>(url, cancellationToken);
        return students ?? [];
    }

    public async Task<EducationStudentProfile?> GetStudentProfileAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/students/{studentId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EducationStudentProfile>(cancellationToken);
    }
}
