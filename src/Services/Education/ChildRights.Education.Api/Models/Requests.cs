using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Domain.Enums;

namespace ChildRights.Education.Api.Models;

public sealed record RecordAttendanceRequest(DateOnly Date, AttendanceStatus Status, string? Subject);

public sealed record RecordGradeRequest(string Subject, int Value, string Term, string? Topic);

/// <summary>Several profiles within a single reform cluster, self-reported by the pupil.</summary>
public sealed record SetDesiredProfilesRequest(IReadOnlyList<EducationProfile> DesiredProfiles);
