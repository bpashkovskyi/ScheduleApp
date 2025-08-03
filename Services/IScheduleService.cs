using ScheduleApp.Models;

namespace ScheduleApp.Services;

public interface IScheduleService
{
    Task<List<Block>> GetBlocksAsync();
    Task<List<ScheduleItem>> GetScheduleAsync(string roomId, DateTime fromDate, DateTime toDate);
    string GetExportUrl(string roomId, DateTime fromDate, DateTime toDate);
    List<PeriodOption> GetPeriodOptions();
} 