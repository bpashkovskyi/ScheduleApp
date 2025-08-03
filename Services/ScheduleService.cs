using System.Text.Json;
using ScheduleApp.Models;

namespace ScheduleApp.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScheduleService> _logger;
    private const string BaseUrl = "https://dekanat.nung.edu.ua/cgi-bin/timetable_export.cgi";
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public ScheduleService(HttpClient httpClient, ILogger<ScheduleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Block>> GetBlocksAsync()
    {
        try
        {
            var url = $"{BaseUrl}?req_type=obj_list&req_mode=room&show_ID=yes&req_format=json&coding_mode=UTF8&bs=ok";
            var response = await _httpClient.GetStringAsync(url);
            var roomListResponse = JsonSerializer.Deserialize<RoomListResponse>(response, JsonOptions);
            
            return roomListResponse?.PsrozkladExport.Blocks.Where(b => b.Objects.Count > 0).ToList() ?? new List<Block>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blocks");
            throw new InvalidOperationException("Помилка завантаження корпусів", ex);
        }
    }

    public async Task<List<ScheduleItem>> GetScheduleAsync(string roomId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var fromDateStr = FormatDate(fromDate);
            var toDateStr = FormatDate(toDate);
            
            var url = $"{BaseUrl}?req_type=rozklad&req_mode=room&OBJ_ID={roomId}&OBJ_name=&dep_name=&ros_text=united&begin_date={fromDateStr}&end_date={toDateStr}&req_format=json&coding_mode=UTF8&bs=ok";
            var response = await _httpClient.GetStringAsync(url);
            var scheduleResponse = JsonSerializer.Deserialize<ScheduleResponse>(response, JsonOptions);
            
            if (scheduleResponse?.PsrozkladExport.Error is not null)
            {
                var errorMessage = scheduleResponse.PsrozkladExport.Error.ErrorMessage ?? "Невідома помилка";
                throw new InvalidOperationException($"Помилка API: {errorMessage}");
            }
            
            return scheduleResponse?.PsrozkladExport.RozItems ?? new List<ScheduleItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedule for room {RoomId}", roomId);
            ////throw new InvalidOperationException("Помилка завантаження розкладу", ex);
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public string GetExportUrl(string roomId, DateTime fromDate, DateTime toDate)
    {
        var fromDateStr = FormatDate(fromDate);
        var toDateStr = FormatDate(toDate);
        
        return $"{BaseUrl}?req_type=rozklad&req_mode=room&OBJ_ID={roomId}&OBJ_name=&dep_name=&ros_text=united&begin_date={fromDateStr}&end_date={toDateStr}&req_format=iCal&coding_mode=UTF8&bs=ok";
    }

    public List<PeriodOption> GetPeriodOptions()
    {
        var today = DateTime.Today;
        var currentWeekStart = GetWeekStart(today);
        var currentWeekEnd = GetWeekEnd(today);
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
        var currentMonthEnd = new DateTime(today.Year, today.Month + 1, 1).AddDays(-1);
        var previousMonthStart = new DateTime(today.Year, today.Month - 1, 1);
        var previousMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);
        
        var currentTerm = GetCurrentTerm(today);
        
        return new List<PeriodOption>
        {
            new()
            {
                Value = "to_end_of_week",
                Label = "До кінця тижня",
                FromDate = today,
                ToDate = currentWeekEnd
            },
            new()
            {
                Value = "current_week",
                Label = "Поточний тиждень",
                FromDate = currentWeekStart,
                ToDate = currentWeekEnd
            },
            new()
            {
                Value = "current_month",
                Label = "Поточний місяць",
                FromDate = currentMonthStart,
                ToDate = currentMonthEnd
            },
            new()
            {
                Value = "previous_month",
                Label = "Попередній місяць",
                FromDate = previousMonthStart,
                ToDate = previousMonthEnd
            },
            new()
            {
                Value = "current_term",
                Label = "Поточний семестр",
                FromDate = currentTerm.Start,
                ToDate = currentTerm.End
            },
            new()
            {
                Value = "custom",
                Label = "Власний період",
                FromDate = today,
                ToDate = today
            }
        };
    }

    private static string FormatDate(DateTime date)
    {
        return $"{date.Day:D2}.{date.Month:D2}.{date.Year}";
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return date.AddDays(-dayOfWeek + (dayOfWeek == 0 ? -6 : 1));
    }

    private static DateTime GetWeekEnd(DateTime date)
    {
        var weekStart = GetWeekStart(date);
        return weekStart.AddDays(6);
    }

    private static (DateTime Start, DateTime End) GetCurrentTerm(DateTime date)
    {
        var month = date.Month;
        var day = date.Day;
        
        // First term: September 1 - December 31 (if current date is from August 20 to January 15)
        if ((month == 8 && day >= 20) || month == 9 || month == 10 || month == 11 || month == 12 || (month == 1 && day <= 15))
        {
            return (
                new DateTime(date.Year, 9, 1), // September 1
                new DateTime(date.Year, 12, 31)  // December 31
            );
        }
        else
        {
            // Second term: February 20 - June 30
            return (
                new DateTime(date.Year, 2, 20), // February 20
                new DateTime(date.Year, 6, 30)    // June 30
            );
        }
    }
} 