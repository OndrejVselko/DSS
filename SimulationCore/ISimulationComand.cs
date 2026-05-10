using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Command interface executed by the simulation before each simulated day.
    /// </summary>
    public interface ISimulationCommand
    {
        /// <summary>
        /// Execute the command against the provided simulation instance.
        /// </summary>
        void Execute(Simulation simulation);
    }
}
