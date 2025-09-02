using System.Text.Json.Serialization;

namespace Schedule.Web.Models;

public sealed class ScheduleExport
{
    [JsonPropertyName("roz_items")]
    public List<Lesson> Lessons { get; set; } = new();
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}