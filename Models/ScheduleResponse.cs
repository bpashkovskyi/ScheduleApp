using System.Text.Json.Serialization;

namespace ScheduleApp.Models;

public sealed record ScheduleResponse
{
    [JsonPropertyName("psrozklad_export")]
    public required PsrozkladExport PsrozkladExport { get; init; }
}

public sealed record PsrozkladExport
{
    [JsonPropertyName("roz_items")]
    public List<ScheduleItem>? RozItems { get; init; }
    
    [JsonPropertyName("error")]
    public ErrorInfo? Error { get; init; }
    
    [JsonPropertyName("code")]
    public string? Code { get; init; }
}

public sealed record ErrorInfo
{
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }
    
    [JsonPropertyName("errorcode")]
    public string? ErrorCode { get; init; }
} 