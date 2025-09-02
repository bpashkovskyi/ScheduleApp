using System.Text.Json.Serialization;

namespace Schedule.Web.Models.Api;

public sealed class ScheduleResponse
{
    [JsonPropertyName("psrozklad_export")]
    public ScheduleExport ScheduleExport { get; set; } = new();
}