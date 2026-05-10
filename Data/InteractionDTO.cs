using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// DTO describing an interaction between a disease ability and a region ability.
    /// </summary>
    public class InteractionDto
    {
        /// <summary>
        /// Id of the related disease ability.
        /// </summary>
        public int DiseaseAbilityId { get; set; }

        /// <summary>
        /// Id of the related region ability.
        /// </summary>
        public int RegionAbilityId { get; set; }

        /// <summary>
        /// Secondary numeric modifier applied when both abilities interact.
        /// </summary>
        public double SecondaryModifier { get; set; }

        /// <summary>
        /// Optional comment describing the interaction.
        /// </summary>
        public string Comment { get; set; } = string.Empty;
    }
}