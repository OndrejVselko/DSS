using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public struct Log
    {
        public int Ordinal { get; set; }
        public DateOnly SimulationDate { get; set; }
        public string Message { get; set; }
        public List<string> Args { get; set; }

        public Log(int ordinal, DateOnly date, string message, params string[] args)
        {
            Ordinal = ordinal;
            SimulationDate = date;
            Message = message;
            if (args != null)
            {
                Args = new List<string>();
                foreach (var arg in args)
                {
                    Args.Add(arg.ToString());
                }
            }
        }

        public string ToString()
        {
            string result = SimulationDate.ToString() + ": " + Message;
            if (Args.Count > 0)
            {
                result += ", Args: ";
                foreach (var arg in Args)
                {
                    result += arg.ToString() + ", ";
                }
            }

            return result;
        }
    }

    public class LogList : List<Log>
    {
        public event Action<Log>? OnLogAdded;

        public void Add(DateOnly date, string message, params string[] args)
        {
            var log = new Log(Count, date, message, args);
            Add(log);
            OnLogAdded?.Invoke(log);
        }
    }
}
