using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Simple log entry used by the simulation.
    /// </summary>
    public class Log
    {
        /// <summary>Log identifier (populated by simulation in the future).</summary>
        public int id {  get; set; }
        /// <summary>Log message text.</summary>
        public string text { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new log entry with the provided text.
        /// </summary>
        public Log(string text) {
            // this.id = (Tady bude funkce simulace, ktera vrati id posledniho logu + 1);
            this.text = text;
        }

        /// <summary>
        /// Returns "id: text".
        /// </summary>
        public override string ToString() { 
            return this.id + ": " + this.text; 
        }
    }
}
