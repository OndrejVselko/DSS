    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// DTO describing an ability (used for deserialization / transport).
    /// </summary>
    public class AbilityDto
    {
        /// <summary>
        /// Unique ability identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ability name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short description of the ability.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Primary numeric modifier for the ability.
        /// </summary>
        public double PrimaryModifier { get; set; }
    }
}