using System.Text.Json.Serialization;

namespace Schedule.Web.Models.Api;

public sealed class ScheduleExport
{
    [JsonPropertyName("roz_items")]
    public List<LessonResponse> RozItems { get; set; } = new();
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}