using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class Log
    {
        public int id {  get; set; }
        public string text { get; set; } = string.Empty;

        public Log(string text) {
            // this.id = (Tady bude funkce simulace, ktera vrati id posledniho logu + 1);
            this.text = text;
        }

        public override string ToString() { 
            return this.id + ": " + this.text; 
        }
    }
}
