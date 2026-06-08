using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Ability that modifies region-specific simulation behaviour.
    /// </summary>
    public class RegionAbility : IAbility
    {
        /// <summary>Ability identifier.</summary>
        public int Id { get; set; }
        /// <summary>Ability name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Ability description.</summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>Primary numeric modifier for the ability.</summary>
        public double SpreadingModifier { get; set; }
        public double DeathModifier { get; set; }
        public double BorderModifier { get; set; }
        public double VaccinationCapacityModifier { get; set; }


        /// <summary>
        /// Initializes a region ability instance.
        /// </summary>
        public RegionAbility(int id, string name, string description, double spreadingModifier, double deathModifier, double borderModifier, double vaccinationCapacityModifier)
        {
            Id = id;
            Name = name;
            Description = description;
            SpreadingModifier = spreadingModifier;
            DeathModifier = deathModifier;
            BorderModifier = borderModifier;
            VaccinationCapacityModifier = vaccinationCapacityModifier;
        }

        /// <summary>
        /// Brief textual representation of the ability.
        /// </summary>
        
        // Double comparing is safe here :)
        public override string ToString()
        {
            string text = $"{Name}";/*, {Description}, modifikátory: ";
            if (SpreadingModifier != 1.0)
                text += "šíření = " + SpreadingModifier + ", ";
            if (DeathModifier != 1.0)
                text += "úmrtnost = " + DeathModifier + ", ";
            if (BorderModifier != 1.0)
                text += "Náhodný výskyt = " + BorderModifier + ", ";
            if (VaccinationCapacityModifier != 1.0)
                text += "Očkování = " + VaccinationCapacityModifier;
            */
            return text;
        }
    }
}
