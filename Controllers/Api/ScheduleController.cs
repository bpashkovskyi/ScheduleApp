using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ScheduleApp.Controllers.Api;

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

    [HttpGet("teacher-load")]
    public async Task<IActionResult> GetTeacherLoad([FromQuery] int teacherId, [FromQuery] int month)
    {
        try
        {
            if (teacherId <= 0)
            {
                return BadRequest(new { error = "Teacher ID must be a positive number" });
            }

            if (month < 1 || month > 12)
            {
                return BadRequest(new { error = "Month must be between 1 and 12" });
            }

            // Get teacher schedule for the specified month
            var scheduleData = await GetTeacherScheduleAsync(teacherId, month);
            
            // Process and categorize the schedule data
            var teacherLoad = ProcessTeacherLoad(scheduleData);

            return Ok(teacherLoad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher load for teacher {TeacherId} in month {Month}", teacherId, month);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<List<ScheduleItem>> GetTeacherScheduleAsync(int teacherId, int month)
    {
        // Construct query to get teacher schedule for the month
        var query = $"req_type=obj&req_mode=teacher&req_format=json&teacher_id={teacherId}&month={month}";
        var url = $"{BaseUrl}?{query}";
        
        var response = await _httpClient.GetStringAsync(url);
        var scheduleData = JsonSerializer.Deserialize<List<ScheduleItem>>(response) ?? new List<ScheduleItem>();
        
        return scheduleData;
    }

    private TeacherLoadResponse ProcessTeacherLoad(List<ScheduleItem> scheduleItems)
    {
        // Filter out cancelled lessons
        var activeItems = scheduleItems.Where(item => 
            !item.LessonDescription.Contains("Увага! Заняття відмінено!", StringComparison.OrdinalIgnoreCase)).ToList();

        // Categorize by employment type
        var hourlyItems = activeItems.Where(item => 
            item.Group.Contains("мз-", StringComparison.Ordinal)).ToList();
        
        var partTimeItems = activeItems.Where(item => 
            !item.Group.Contains("мз-", StringComparison.Ordinal) && 
            item.Group.Contains("з-", StringComparison.Ordinal)).ToList();
        
        var fullTimeItems = activeItems.Where(item => 
            !item.Group.Contains("мз-", StringComparison.Ordinal) && 
            !item.Group.Contains("з-", StringComparison.Ordinal)).ToList();

        return new TeacherLoadResponse
        {
            TeacherId = scheduleItems.FirstOrDefault()?.TeacherId ?? 0,
            Month = scheduleItems.FirstOrDefault()?.Date?.Month.ToString() ?? "1",
            FullTime = CategorizeByLessonType(fullTimeItems),
            PartTime = CategorizeByLessonType(partTimeItems),
            Hourly = CategorizeByLessonType(hourlyItems)
        };
    }

    private LessonCategories CategorizeByLessonType(List<ScheduleItem> items)
    {
        return new LessonCategories
        {
            Lectures = CreateLessonObjects(items.Where(item => item.Group.Contains("(Л)")).ToList()),
            Practice = CreateLessonObjects(items.Where(item => item.Group.Contains("(Пр)")).ToList()),
            Labs = CreateLessonObjects(items.Where(item => item.Group.Contains("(Лаб)")).ToList()),
            Credits = CreateLessonObjects(items.Where(item => item.Group.Contains("(Зал)")).ToList()),
            ExamConsultations = CreateLessonObjects(items.Where(item => item.Group.Contains("(КЕкз)")).ToList()),
            Exams = CreateLessonObjects(items.Where(item => item.Group.Contains("(Екз)")).ToList())
        };
    }

    private List<LessonObject> CreateLessonObjects(List<ScheduleItem> items)
    {
        return items.Select(item => new LessonObject
        {
            Description = $"{item.LessonDescription} {item.Group} {item.Date:dd.MM.yyyy} {item.LessonTime}",
            Hours = CalculateHours(item.LessonDescription)
        }).ToList();
    }

    private int CalculateHours(string lessonDescription)
    {
        if (lessonDescription.Contains("Увага! Заняття відмінено!", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }
        
        if (lessonDescription.Contains("півпара", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }
        
        return 2; // Default hours
    }
}

// Data models
public sealed record TeacherLoadResponse
{
    public int TeacherId { get; init; }
    public string Month { get; init; } = string.Empty;
    public LessonCategories FullTime { get; init; } = new();
    public LessonCategories PartTime { get; init; } = new();
    public LessonCategories Hourly { get; init; } = new();
}

public sealed record LessonCategories
{
    public List<LessonObject> Lectures { get; init; } = new();
    public List<LessonObject> Practice { get; init; } = new();
    public List<LessonObject> Labs { get; init; } = new();
    public List<LessonObject> Credits { get; init; } = new();
    public List<LessonObject> ExamConsultations { get; init; } = new();
    public List<LessonObject> Exams { get; init; } = new();
}

public sealed record LessonObject
{
    public string Description { get; init; } = string.Empty;
    public int Hours { get; init; }
}

public sealed record ScheduleItem
{
    public int TeacherId { get; init; }
    public string Group { get; init; } = string.Empty;
    public string LessonDescription { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string LessonTime { get; init; } = string.Empty;
} 