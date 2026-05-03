using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class RegionAbility : IAbility
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double PrimaryModifier { get; set; }

        public RegionAbility(int id, string name, string description, double primaryModifier)
        {
            Id = id;
            Name = name;
            Description = description;
            PrimaryModifier = primaryModifier;
        }

        public override string ToString()
        {
            return $"{Name}, {Description}, modifikátor: {PrimaryModifier}";
        }
    }
}
