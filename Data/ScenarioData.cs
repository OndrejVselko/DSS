using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimulationCore;

namespace Data
{
    public class ScenarioData
    {
        public string ScenarioName { get; set; } = string.Empty;
        public List<Region> Regions { get; set; } = new();
        public List<RegionAbility> RegionAbilities { get; set; } = new();
        public List<DiseaseAbility> DiseaseAbilities { get; set; } = new();
        public List<InteractionDto> Interactions { get; set; } = new();
    }

    public class InteractionDto
    {
        public int DiseaseAbilityId { get; set; }
        public int RegionAbilityId { get; set; }
        public double SecondaryModifier { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}