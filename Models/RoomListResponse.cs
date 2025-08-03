using System.Text.Json.Serialization;

namespace ScheduleApp.Models;

public sealed record RoomListResponse
{
    [JsonPropertyName("psrozklad_export")]
    public required PsrozkladExportRoomList PsrozkladExport { get; init; }
}

public sealed record PsrozkladExportRoomList
{
    [JsonPropertyName("blocks")]
    public required List<Block> Blocks { get; init; }
} 