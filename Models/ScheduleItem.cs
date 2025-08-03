using System.Text.Json.Serialization;

namespace ScheduleApp.Models;

public sealed record ScheduleItem
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }
    
    [JsonPropertyName("date")]
    public required string Date { get; init; }
    
    [JsonPropertyName("comment")]
    public required string Comment { get; init; }
    
    [JsonPropertyName("lesson_number")]
    public required string LessonNumber { get; init; }
    
    [JsonPropertyName("lesson_name")]
    public required string LessonName { get; init; }
    
    [JsonPropertyName("lesson_time")]
    public required string LessonTime { get; init; }
    
    [JsonPropertyName("lesson_description")]
    public required string LessonDescription { get; init; }
} 