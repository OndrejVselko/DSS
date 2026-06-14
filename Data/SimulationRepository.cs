using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data
{
    public class SimulationRepository
    {
        public async Task SaveSimulationAsync(SimulationRecord record, List<LogEntry> entries)
        {
            using var context = new SimulationDbContext();
            record.LogEntries = entries;
            context.SimulationRecords.Add(record);
            await context.SaveChangesAsync();
        }

        public async Task<List<SimulationRecord>> GetAllSimulationsAsync()
        {
            using var context = new SimulationDbContext();
            return await context.SimulationRecords
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public List<LogEntry> GetLogs(int simulationId)
        {
            using var context = new SimulationDbContext();
            return context.LogEntries
                .Where(e => e.SimulationRecordId == simulationId)
                .AsEnumerable()
                .OrderBy(e => DateOnly.Parse(e.Day, new System.Globalization.CultureInfo("cs-CZ")))
                .ToList();
        }
    }
}