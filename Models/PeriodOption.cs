namespace ScheduleApp.Models;

public sealed record PeriodOption
{
    public required string Value { get; init; }
    public required string Label { get; init; }
    public required DateTime FromDate { get; init; }
    public required DateTime ToDate { get; init; }
} 