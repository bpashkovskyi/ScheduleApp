using FastEndpoints;
using Schedule.Web.Services;
using Schedule.Web.Models.Domain;
using Schedule.Web.Models.Domain.Enums;

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
            lessons = lessons.Where(lesson => !string.IsNullOrEmpty(lesson.LessonDescription)).ToList();

            // Group lessons by type and hour type
            var response = new TeacherScheduleResponse
            {
                TeacherId = request.TeacherId,
                MonthId = request.MonthId,
                FullTime = GroupLessonsByType(lessons.Where(l => l.HourType == HourType.FullTime)),
                PartTime = GroupLessonsByType(lessons.Where(l => l.HourType == HourType.PartTime)),
                Hourly = GroupLessonsByType(lessons.Where(l => l.HourType == HourType.Hourly))
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

    private static HourTypeSchedule GroupLessonsByType(IEnumerable<Lesson> lessons)
    {
        var schedule = new HourTypeSchedule();
        
        foreach (var lesson in lessons)
        {
            var lessonItem = new LessonItem
            {
                Description = $"{lesson.LessonDescription} {lesson.Date} {lesson.LessonTime}",
                Hours = lesson.LessonHours
            };

            switch (lesson.LessonType)
            {
                case LessonType.Lecture:
                    schedule.Lectures.Add(lessonItem);
                    break;
                case LessonType.Practical:
                    schedule.Practice.Add(lessonItem);
                    break;
                case LessonType.Laboratory:
                    schedule.Labs.Add(lessonItem);
                    break;
                case LessonType.Credit:
                    schedule.Credits.Add(lessonItem);
                    break;
                case LessonType.ExamConsultation:
                    schedule.ExamConsultations.Add(lessonItem);
                    break;
                case LessonType.Exam:
                    schedule.Exams.Add(lessonItem);
                    break;
                default:
                    // For lessons without a specific type, add to lectures as default
                    schedule.Lectures.Add(lessonItem);
                    break;
            }
        }

        return schedule;
    }
}
