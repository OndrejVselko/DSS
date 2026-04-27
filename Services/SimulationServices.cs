using System;
using SimulationCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SimulationServices
    {
        Simulation simulation { get; set; }
        public SimulationServices() { 
        }

        public void setSimulation(Disease disease, List<Region> regions)
        {
            simulation = new Simulation(disease, regions);
        }
    }
}
