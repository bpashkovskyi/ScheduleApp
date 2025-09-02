using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Schedule.Web.Services;

namespace Schedule.Web.Endpoints.TeacherSchedule;

public sealed class TeacherScheduleEndpoint : Endpoint<TeacherScheduleRequest, TeacherScheduleResponse>
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<TeacherScheduleEndpoint> _logger;

    public TeacherScheduleEndpoint(IScheduleService scheduleService, ILogger<TeacherScheduleEndpoint> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/schedule/teacher-schedule");
        AllowAnonymous();
        Description(d => d
            .WithName("GetTeacherSchedule")
            .WithTags("Schedule")
            .WithSummary("Get teacher schedule for a specific month")
            .WithDescription("Retrieves the schedule for a specific teacher in a given month"));
    }

    public override async Task HandleAsync(TeacherScheduleRequest req, CancellationToken ct)
    {
        try
        {
            if (req.TeacherId <= 0)
            {
                AddError(r => r.TeacherId, "Teacher ID must be a positive number");
                await SendErrorsAsync(400, ct);
                return;
            }

            if (req.MonthId is < 1 or > 12)
            {
                AddError(r => r.MonthId, "Month ID must be between 1 and 12");
                await SendErrorsAsync(400, ct);
                return;
            }

            // Get teacher schedule data from external API
            var lessons = await _scheduleService.GetTeacherScheduleDataAsync(req.TeacherId, req.MonthId);

            var response = new TeacherScheduleResponse
            {
                TeacherId = req.TeacherId,
                MonthId = req.MonthId,
                LessonsCount = lessons.Count,
                Lessons = lessons
            };

            await SendAsync(response, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher schedule for teacher {TeacherId} in month {MonthId}", req.TeacherId, req.MonthId);
            AddError("An error occurred while processing the request");
            await SendErrorsAsync(500, ct);
        }
    }
}
