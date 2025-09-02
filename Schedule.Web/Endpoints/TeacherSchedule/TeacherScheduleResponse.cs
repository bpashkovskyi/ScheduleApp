using System.Text.Json.Serialization;

namespace Schedule.Web.Endpoints.TeacherSchedule;

public sealed class TeacherScheduleResponse
{
    [JsonPropertyName("teacher_id")]
    public int TeacherId { get; set; }
    
    [JsonPropertyName("month_id")]
    public int MonthId { get; set; }
    
    [JsonPropertyName("full_time")]
    public HourTypeSchedule FullTime { get; set; } = new();
    
    [JsonPropertyName("part_time")]
    public HourTypeSchedule PartTime { get; set; } = new();
    
    [JsonPropertyName("hourly")]
    public HourTypeSchedule Hourly { get; set; } = new();
}