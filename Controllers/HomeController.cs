using Microsoft.AspNetCore.Mvc;
using ScheduleApp.Models;
using ScheduleApp.Services;

namespace ScheduleApp.Controllers;

public sealed class HomeController : Controller
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IScheduleService scheduleService, ILogger<HomeController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var blocks = await _scheduleService.GetBlocksAsync();
            var periodOptions = _scheduleService.GetPeriodOptions();

            var viewModel = new ScheduleViewModel
            {
                Blocks = blocks,
                PeriodOptions = periodOptions,
                SelectedPeriod = "to_end_of_week"
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading initial data");
            var viewModel = new ScheduleViewModel
            {
                ErrorMessage = "Помилка завантаження даних: " + ex.Message
            };
            return View(viewModel);
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetRooms(string selectedBlock)
    {
        try
        {
            var blocks = await _scheduleService.GetBlocksAsync();
            var selectedBlockData = blocks.FirstOrDefault(b => b.Name == selectedBlock);

            var availableRooms = selectedBlockData?.Objects ?? new List<RoomObject>();

            return Json(new { success = true, rooms = availableRooms });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rooms for block {Block}", selectedBlock);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetSchedule([FromBody] ScheduleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RoomId) || !request.FromDate.HasValue || !request.ToDate.HasValue)
            {
                return Json(new { success = false, error = "Необхідно заповнити всі поля" });
            }

            var scheduleItems =
                await _scheduleService.GetScheduleAsync(request.RoomId, request.FromDate.Value, request.ToDate.Value);
            var exportUrl = _scheduleService.GetExportUrl(request.RoomId, request.FromDate.Value, request.ToDate.Value);

            // Get room name
            var blocks = await _scheduleService.GetBlocksAsync();
            var roomName = blocks
                .SelectMany(b => b.Objects)
                .FirstOrDefault(r => r.Id == request.RoomId)?.Name ?? "Невідома аудиторія";

            var groupedSchedule = GroupScheduleByDate(scheduleItems);

            return Json(new
            {
                success = true,
                schedule = groupedSchedule,
                roomName = roomName,
                exportUrl = exportUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedule");
            return Json(new { success = false, error = ex.Message });
        }
    }

    private static Dictionary<string, List<ScheduleItem>> GroupScheduleByDate(List<ScheduleItem> scheduleItems)
    {
        return scheduleItems
            .GroupBy(item => item.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public IActionResult Error()
    {
        return View();
    }
}

public sealed record ScheduleRequest
{
    public string? RoomId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
} 