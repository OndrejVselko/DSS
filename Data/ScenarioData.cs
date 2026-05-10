
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimulationCore;

namespace Data
{
    /// <summary>
    /// Container for scenario data loaded from JSON (regions, abilities, interactions).
    /// </summary>
    public class ScenarioData
    {
        /// <summary>
        /// List of regions included in the scenario.
        /// </summary>
        public List<SimulationCore.Region> Regions { get; set; } = new();

        /// <summary>
        /// Abilities applicable to regions (DTO form).
        /// </summary>
        public List<AbilityDto> RegionAbilities { get; set; } = new();

        /// <summary>
        /// Abilities applicable to diseases (DTO form).
        /// </summary>
        public List<AbilityDto> DiseaseAbilities { get; set; } = new();

        /// <summary>
        /// Interactions between disease and region abilities.
        /// </summary>
        public List<InteractionDto> Interactions { get; set; } = new();
    }
}