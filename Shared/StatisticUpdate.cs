using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public int TotalSick { get; set; }
        public int NewSick { get; set; }
        public int TotalDead { get; set; }
        public int NewDead { get; set; }
        public int TotalVaccinated { get; set; }
        public int NewVaccinated { get; set; }

        public Dictionary<string, Region> RegionsByIso { get; set; } = new();

        /// <summary>
        /// Creates a statistic update record.
        /// </summary>
        public StatisticUpdate(int newSick, int newDead, int newVaccinated, int totalSick, int totalDead, int totalVaccinated)
        {
            TotalSick = totalSick;
            TotalDead = totalDead;
            TotalVaccinated = totalVaccinated;
            NewSick = newSick;
            NewDead = newDead;
            NewVaccinated = newVaccinated;
        }
        public StatisticUpdate(DateOnly date, int newSick, int newDead, int newVaccinated, int totalSick, int totalDead, int totalVaccinated, Dictionary<string, Region> regionsByIso)
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
            return $"Nakazeni: {TotalSick} ({NewSick}); Mrtvi: {TotalDead} ({NewDead}); Ockovani: {TotalVaccinated} ({NewVaccinated})";
        } 
    }
}
