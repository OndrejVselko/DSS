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
        public double SpreadingModifier { get; set; }
        public double DeathModifier { get; set; }
        public double BorderModifier { get; set; }
        public double VaccinationCapacityModifier { get; set; }

        /// <summary>
        /// Initializes a disease ability instance.
        /// </summary>
        public DiseaseAbility(int id, string name, string description, double spreadingModifier, double deathModifier, double borderModifier, double vaccinationCapacityModifier)
        {
            Id = id;
            Name = name;
            Description = description;
            SpreadingModifier = spreadingModifier;
            DeathModifier = deathModifier;
            BorderModifier = borderModifier;
            VaccinationCapacityModifier = vaccinationCapacityModifier;
        }

        public override string ToString()
        {
            string text = $"{Name}";

            return text;
        }
    }

}
