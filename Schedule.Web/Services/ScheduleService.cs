using System.Text.Json;
using Schedule.Web.Models.Api;
using Schedule.Web.Models.Domain;

namespace Schedule.Web.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScheduleService> _logger;
    private readonly ILessonMapper _lessonMapper;
    private const string BaseUrl = "https://dekanat.nung.edu.ua/cgi-bin/timetable_export.cgi";

    public ScheduleService(HttpClient httpClient, ILogger<ScheduleService> logger, ILessonMapper lessonMapper)
    {
        _httpClient = httpClient;
        _logger = logger;
        _lessonMapper = lessonMapper;
    }

    public async Task<string> GetProxyDataAsync(string query)
    {
        var url = $"{BaseUrl}?{query}";
        return await _httpClient.GetStringAsync(url);
    }

    public async Task<List<Lesson>> GetTeacherScheduleDataAsync(int teacherId, int monthId)
    {
        // Convert month ID to date range in dd.MM.yyyy format
        var (beginDate, endDate) = GetMonthDateRange(monthId);
        
        // Construct query using OBJ_ID for teacher and date range format
        var query = $"req_type=rozklad&req_mode=teacher&OBJ_ID={teacherId}&OBJ_name=&dep_name=&ros_text=united&begin_date={beginDate}&end_date={endDate}&req_format=json&coding_mode=UTF8&bs=ok";
        var url = $"{BaseUrl}?{query}";

        var response = await _httpClient.GetStringAsync(url);
        
        try
        {
            var scheduleResponse = JsonSerializer.Deserialize<ScheduleResponse>(response);
            var lessonResponses = scheduleResponse?.ScheduleExport?.RozItems ?? new List<LessonResponse>();
            
            // Convert API response models to domain models
            return _lessonMapper.ToDomainList(lessonResponses);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Error deserializing schedule response for teacher {TeacherId} in month {MonthId}", teacherId, monthId);
            return new List<Lesson>();
        }
    }

    private static (string beginDate, string endDate) GetMonthDateRange(int monthId)
    {
        var currentYear = DateTime.Now.Year;
        var month = monthId;
        var year = currentYear;
        
        // If month is in the past for current year, use next year
        if (month < DateTime.Now.Month)
        {
            year++;
        }
        
        var firstDayOfMonth = new DateTime(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        
        return (firstDayOfMonth.ToString("dd.MM.yyyy"), lastDayOfMonth.ToString("dd.MM.yyyy"));
    }
}
