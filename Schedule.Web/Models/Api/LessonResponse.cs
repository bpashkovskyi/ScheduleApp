using System.Text.Json.Serialization;

namespace Schedule.Web.Models.Api;

public sealed class LessonResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;
    
    [JsonPropertyName("lesson_number")]
    public string LessonNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("lesson_name")]
    public string LessonName { get; set; } = string.Empty;
    
    [JsonPropertyName("lesson_time")]
    public string LessonTime { get; set; } = string.Empty;
    
    [JsonPropertyName("lesson_description")]
    public string LessonDescription { get; set; } = string.Empty;
}
