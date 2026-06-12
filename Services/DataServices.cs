using Data;
using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Services
{
    /// <summary>
    /// Simple record representing loaded scenario data in typed dictionaries.
    /// </summary>
    public record LoadedScenario(
        Dictionary<int, Region> Regions,
        Dictionary<int, DiseaseAbility> DiseaseAbilities,
        Dictionary<int, RegionAbility> RegionAbilities,
        Dictionary<(int, int), Interaction> Interactions
    );

    /// <summary>
    /// Service that converts raw DTO scenario data into domain objects using factories.
    /// </summary>
    public class DataServices
    {
        /// <summary>
        /// Factory used to create typed ability instances from DTOs.
        /// </summary>
        private readonly AbilityFactory _abilityFactory;

        /// <summary>
        /// Initializes the ability factory.
        /// </summary>
        public DataServices()
        {
            _abilityFactory = new AbilityFactory();
        }

        /// <summary>
        /// Loads scenario from JSON and converts DTOs to domain objects.
        /// </summary>
        public async Task<LoadedScenario> LoadScenario(string path)
        {
            ScenarioData data = await JsonParser.LoadScenarioFromJson(path);

            var regions = data.Regions.ToDictionary(r => r.Id);

            var diseaseAbilities = data.DiseaseAbilities
                .Select(dto => _abilityFactory.CreateDisease(dto))
                .ToDictionary(a => a.Id);

            var regionAbilities = data.RegionAbilities
                .Select(dto => _abilityFactory.CreateRegion(dto))
                .ToDictionary(a => a.Id);

            var interactions = data.Interactions.ToDictionary(
                i => (i.DiseaseAbilityId, i.RegionAbilityId),
                i => new Interaction(i.DiseaseAbilityId, i.RegionAbilityId, i.SpreadingModifier, i.DeathModifier, i.Comment)
            );

            return new LoadedScenario(regions, diseaseAbilities, regionAbilities, interactions);
        }


        private readonly SimulationRepository _repository = new();

        public async Task SaveSimulationAsync(SimulationRecord record, List<LogEntry> entries)
            => await _repository.SaveSimulationAsync(record, entries);

        public async Task<List<SimulationRecord>> GetAllSimulationsAsync()
            => await _repository.GetAllSimulationsAsync();

        public void EnsureDatabase() => SimulationDbContext.EnsureCreated();

        public async Task<List<LogEntry>> GetLogs(int simulationId) => _repository.GetLogsAsync(simulationId);
    }
}