using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public struct StatisticUpdate
    {
        public int TotalSick;
        public int TotalDead;
        public int TotalVaccinated;
        public int NewSick;
        public int NewDead;
        public int NewVaccinated;

        public StatisticUpdate(int newSick, int newDead, int newVaccinated, int totalSick, int totalDead, int totalVaccinated)
        {
            TotalSick = totalSick;
            TotalDead = totalDead;
            TotalVaccinated = totalVaccinated;
            NewSick = newSick;
            NewDead = newDead;
            NewVaccinated = newVaccinated;
        }

        public override string ToString()
        {
            return $"Nakazeni: {TotalSick} ({NewSick}); Mrtvi: {TotalDead} ({NewDead}); Ockovani: {TotalVaccinated} ({NewVaccinated})";
        } 
    }
}
