using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public struct StatisticUpdate
    {
        public int newSick;
        public int newDead;
        public int newVaccinated;

        public StatisticUpdate(int newSick, int newDead, int newVaccinated)
        {
            this.newSick = newSick;
            this.newDead = newDead;
            this.newVaccinated = newVaccinated;
        }

        public override string ToString()
        {
            return "Nakazeni: " + newSick + " Mrtvi: " + newDead + " Ockovani: " + newVaccinated;
        } 
    }
}
