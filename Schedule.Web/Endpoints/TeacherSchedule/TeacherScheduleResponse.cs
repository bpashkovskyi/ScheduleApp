using Schedule.Web.Models.Domain;

namespace Schedule.Web.Endpoints.TeacherSchedule;

public sealed class TeacherScheduleResponse
{
    public int TeacherId { get; set; }
    public int MonthId { get; set; }
    public int LessonsCount { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
}
