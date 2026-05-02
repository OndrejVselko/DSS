using Data;
using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services
{
    public record LoadedScenario(
        Dictionary<int, Region> Regions,
        Dictionary<int, DiseaseAbility> DiseaseAbilities,
        Dictionary<int, RegionAbility> RegionAbilities,
        Dictionary<(int, int), Interaction> Interactions
    );

    public class DataServices
    {

        public DataServices() { 
        
        }
  

        public async Task<LoadedScenario> LoadScenario(string path)
        {
            ScenarioData data = await JsonParser.LoadScenarioFromJson(path);

            var regions = data.Regions.ToDictionary(r => r.Id);
            var diseaseAbilities = data.DiseaseAbilities.ToDictionary(a => a.Id);
            var regionAbilities = data.RegionAbilities.ToDictionary(a => a.Id);
            var interactions = data.Interactions.ToDictionary(
                i => (i.DiseaseAbilityId, i.RegionAbilityId),
                i => new Interaction(i.DiseaseAbilityId, i.RegionAbilityId, i.SecondaryModifier, i.Comment)
            );

            return new LoadedScenario(regions, diseaseAbilities, regionAbilities, interactions);
        }

    }
}
