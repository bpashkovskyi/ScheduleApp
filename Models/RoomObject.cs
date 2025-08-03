using System.Text.Json.Serialization;

namespace ScheduleApp.Models;

public sealed record RoomObject
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("ID")]
    public required string Id { get; init; }
} 