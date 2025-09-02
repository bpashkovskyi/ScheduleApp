using FastEndpoints;
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
        Description(description => description
            .WithName("GetTeacherSchedule")
            .WithTags("Schedule")
            .WithGroupName("Schedule API")
            .WithSummary("Get teacher schedule for a specific month")
            .WithDescription("Retrieves the schedule for a specific teacher in a given month"));
    }

    public override async Task HandleAsync(TeacherScheduleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.TeacherId <= 0)
            {
                AddError(teacherScheduleRequest => teacherScheduleRequest.TeacherId, "Teacher ID must be a positive number");
                await SendErrorsAsync(400, cancellationToken);
                return;
            }

            if (request.MonthId is < 1 or > 12)
            {
                AddError(teacherScheduleRequest => teacherScheduleRequest.MonthId, "Month ID must be between 1 and 12");
                await SendErrorsAsync(400, cancellationToken);
                return;
            }

            // Get teacher schedule data from external API
            var lessons = await _scheduleService.GetTeacherScheduleDataAsync(request.TeacherId, request.MonthId);

            var response = new TeacherScheduleResponse
            {
                TeacherId = request.TeacherId,
                MonthId = request.MonthId,
                LessonsCount = lessons.Count,
                Lessons = lessons
            };

            await SendAsync(response, cancellation: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error getting teacher schedule for teacher {TeacherId} in month {MonthId}", request.TeacherId, request.MonthId);
            AddError("An error occurred while processing the request");
            await SendErrorsAsync(500, cancellationToken);
        }
    }
}
