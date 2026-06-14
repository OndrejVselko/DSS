using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Snapshot of statistic changes produced by a simulation day.
    /// </summary>
    public struct StatisticUpdate
    {
        public DateOnly Date { get; set; }
        public long TotalSick { get; set; }
        public long NewSick { get; set; }
        public long TotalDead { get; set; }
        public long NewDead { get; set; }
        public long TotalVaccinated { get; set; }
        public long NewVaccinated { get; set; }

        public Dictionary<string, Region> RegionsByIso { get; set; } = new();

        /// <summary>
        /// Creates a statistic update record.
        /// </summary>
        public StatisticUpdate(long newSick, long newDead, long newVaccinated, long totalSick, long totalDead, long totalVaccinated)
        {
            TotalSick = totalSick;
            TotalDead = totalDead;
            TotalVaccinated = totalVaccinated;
            NewSick = newSick;
            NewDead = newDead;
            NewVaccinated = newVaccinated;
        }
        public StatisticUpdate(DateOnly date, long newSick, long newDead, long newVaccinated, long totalSick, long totalDead, long totalVaccinated, Dictionary<string, Region> regionsByIso)
        {
            Date = date;
            TotalSick = totalSick;
            TotalDead = totalDead;
            TotalVaccinated = totalVaccinated;
            NewSick = newSick;
            NewDead = newDead;
            NewVaccinated = newVaccinated;
            RegionsByIso = regionsByIso;
        }

        /// <summary>
        /// Human readable representation of statistics (Czech).
        /// </summary>
        public override string ToString()
        {
            return $"Inf.: {TotalSick} ({NewSick}); Mrtvi: {TotalDead} ({NewDead}); Ock.: {TotalVaccinated} ({NewVaccinated})";
        } 
    }
}
