using System.Text.Json.Serialization;
using Schedule.Web.Models.Api;

namespace Schedule.Web.Models;

public sealed class ScheduleResponse
{
    [JsonPropertyName("psrozklad_export")]
    public ScheduleExport ScheduleExport { get; set; } = new();
}

public sealed class ScheduleExport
{
    [JsonPropertyName("roz_items")]
    public List<LessonResponse> RozItems { get; set; } = new();
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}