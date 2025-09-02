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
} 