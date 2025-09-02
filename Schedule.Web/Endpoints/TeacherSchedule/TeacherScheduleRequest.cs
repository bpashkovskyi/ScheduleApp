using FastEndpoints;

namespace Schedule.Web.Endpoints.TeacherSchedule;

public sealed class TeacherScheduleRequest
{
    public int TeacherId { get; set; }
    public int MonthId { get; set; }
}
