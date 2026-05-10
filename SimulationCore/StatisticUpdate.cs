using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Snapshot of statistic changes produced by a simulation day.
    /// </summary>
    public struct StatisticUpdate
    {
        /// <summary>Current total sick count.</summary>
        public int TotalSick;
        /// <summary>Current total dead count.</summary>
        public int TotalDead;
        /// <summary>Current total vaccinated count.</summary>
        public int TotalVaccinated;
        /// <summary>New sick during the last simulated period.</summary>
        public int NewSick;
        /// <summary>New dead during the last simulated period.</summary>
        public int NewDead;
        /// <summary>New vaccinated during the last simulated period.</summary>
        public int NewVaccinated;

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

        /// <summary>
        /// Human readable representation of statistics (Czech).
        /// </summary>
        public override string ToString()
        {
            return $"Nakazeni: {TotalSick} ({NewSick}); Mrtvi: {TotalDead} ({NewDead}); Ockovani: {TotalVaccinated} ({NewVaccinated})";
        } 
    }
}
