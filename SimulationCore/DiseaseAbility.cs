using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Ability that modifies disease-specific simulation behaviour.
    /// </summary>
    public class DiseaseAbility :IAbility
    {
        /// <summary>Ability identifier.</summary>
        public int Id { get; set; }
        /// <summary>Ability name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Ability description.</summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>Primary numeric modifier for the ability.</summary>
        public double PrimaryModifier { get; set; }

        /// <summary>
        /// Initializes a disease ability instance.
        /// </summary>
        public DiseaseAbility(int id, string name, string description, double primaryModifier)
        {
            Id = id;
            Name = name;
            Description = description;
            PrimaryModifier = primaryModifier;
        }
    }
}
