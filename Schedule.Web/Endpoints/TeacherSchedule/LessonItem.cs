using System.Text.Json.Serialization;

namespace Schedule.Web.Endpoints.TeacherSchedule;

public sealed class LessonItem
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("hours")]
    public int Hours { get; set; }
}