using System.ComponentModel.DataAnnotations;

namespace ScheduleApp.Models;

public sealed record ScheduleViewModel
{
    public List<Block> Blocks { get; init; } = new();
    public List<RoomObject> AvailableRooms { get; init; } = new();
    public List<PeriodOption> PeriodOptions { get; init; } = new();
    
            [Display(Name = "Корпус")]
    public string? SelectedBlock { get; init; }
    
    [Display(Name = "Аудиторія")]
    public string? SelectedRoom { get; init; }
    
    [Display(Name = "Період")]
    public string? SelectedPeriod { get; init; }
    
    [Display(Name = "Від дати")]
    public DateTime? FromDate { get; init; }
    
    [Display(Name = "До дати")]
    public DateTime? ToDate { get; init; }
    
    public List<ScheduleItem> ScheduleItems { get; init; } = new();
    public string? SelectedRoomName { get; init; }
    public string? ExportUrl { get; init; }
    public string? ErrorMessage { get; init; }
} 