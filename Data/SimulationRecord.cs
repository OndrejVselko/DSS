using System;
using System.Collections.Generic;

namespace Data
{
    public class SimulationRecord
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public double DefaultSpreadingSpeed { get; set; }
        public double DefaultDeathProbability { get; set; }
        public int SicknessLength { get; set; }
        public int ImmunityLength { get; set; }

        public List<LogEntry> LogEntries { get; set; } = new();
    }

    public class LogEntry
    {
        public int Id { get; set; }
        public int SimulationRecordId { get; set; }
        public string Day { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public SimulationRecord SimulationRecord { get; set; } = null!;
    }
}

