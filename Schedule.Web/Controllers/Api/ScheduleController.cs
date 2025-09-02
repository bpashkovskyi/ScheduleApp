using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Schedule.Web.Models;

namespace Schedule.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public sealed class ScheduleController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScheduleController> _logger;
    private const string BaseUrl = "https://dekanat.nung.edu.ua/cgi-bin/timetable_export.cgi";

    public ScheduleController(HttpClient httpClient, ILogger<ScheduleController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet("proxy")]
    public async Task<IActionResult> Proxy([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest(new { error = "Query parameter 'q' is required" });
            }

            var url = $"{BaseUrl}?{q}";
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying request to external API");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("teacher-schedule")]
    public async Task<IActionResult> GetTeacherSchedule([FromQuery] int teacherId, [FromQuery] int monthId)
    {
        try
        {
            if (teacherId <= 0)
            {
                return BadRequest(new { error = "Teacher ID must be a positive number" });
            }

            if (monthId is < 1 or > 12)
            {
                return BadRequest(new { error = "Month ID must be between 1 and 12" });
            }

            // Get teacher schedule data from external API
            var lessons = await GetTeacherScheduleDataAsync(teacherId, monthId);

            return Ok(new
            {
                teacherId,
                monthId,
                lessonsCount = lessons.Count,
                lessons
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher schedule for teacher {TeacherId} in month {MonthId}", teacherId,
                monthId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<List<Lesson>> GetTeacherScheduleDataAsync(int teacherId, int monthId)
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
            return scheduleResponse?.ScheduleExport?.Lessons ?? new List<Lesson>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing schedule response for teacher {TeacherId} in month {MonthId}", teacherId, monthId);
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