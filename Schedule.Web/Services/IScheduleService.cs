using Schedule.Web.Models;

namespace Schedule.Web.Services;

public interface IScheduleService
{
    Task<string> GetProxyDataAsync(string query);
    Task<List<Lesson>> GetTeacherScheduleDataAsync(int teacherId, int monthId);
}
