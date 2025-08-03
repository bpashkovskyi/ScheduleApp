using System.Text.Json.Serialization;

namespace ScheduleApp.Models;

public sealed record Block
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("objects")]
    public required List<RoomObject> Objects { get; init; }
} 