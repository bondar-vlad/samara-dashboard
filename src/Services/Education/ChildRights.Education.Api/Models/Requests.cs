using ChildRights.Education.Domain.Enums;

namespace ChildRights.Education.Api.Models;

public sealed record RecordAttendanceRequest(DateOnly Date, AttendanceStatus Status, string? Subject);

public sealed record RecordGradeRequest(string Subject, int Value, string Term);
