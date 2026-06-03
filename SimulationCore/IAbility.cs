using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Common interface for abilities used by diseases and regions.
    /// </summary>
    public interface IAbility
    {
        /// <summary>Ability identifier.</summary>
        public int Id { get; set; }
        /// <summary>Ability name.</summary>
        public string Name { get; set; }
        /// <summary>Ability description.</summary>
        public string Description { get; set; }
        /// <summary>Primary numeric modifier.</summary>
        public double SpreadingModifier { get; set; }
        public double DeathModifier { get; set; }
        public double BorderModifier { get; set; }
        public double VaccinationCapacityModifier { get; set; }
    }
}
