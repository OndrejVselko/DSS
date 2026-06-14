using Data;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Shared;
using SimulationCore;

namespace Services
{
    /// <summary>
    /// Facade service that composes data and simulation services for the UI.
    /// </summary>
    public class AppService
    {

        // EVENTS
        public event Action<StatisticUpdate>? OnDaySimulated;
        public event Action<Log>? OnLogAdded;

        /// <summary>
        /// Service responsible for running the simulation.
        /// </summary>
        private readonly SimulationServices _simulationServices;

        /// <summary>
        /// Service responsible for loading scenario data.
        /// </summary>
        private readonly DataServices _dataServices;

        /// <summary>
        /// Available disease abilities loaded from data.
        /// </summary>
        private Dictionary<int, DiseaseAbility> _availableDiseaseAbilities;

        /// <summary>
        /// Available region abilities loaded from data.
        /// </summary>
        private Dictionary<int, RegionAbility> _availableRegionAbilities;

        /// <summary>
        /// Interaction mapping between disease and region abilities.
        /// </summary>
        private Dictionary<(int , int), Interaction> _interaction;

        /// <summary>
        /// Initializes services and subscribes event forwarding.
        /// </summary>
        public AppService()
        {
            _simulationServices = new SimulationServices();
            _dataServices = new DataServices();
            _simulationServices.OnDaySimulated += update => OnDaySimulated?.Invoke(update);
            _simulationServices.OnLogAdded += log => OnLogAdded?.Invoke(log);

            _availableDiseaseAbilities = new();
            _availableRegionAbilities = new();
            _interaction = new();
        }

        // DATA METHODS
        public async Task LoadData(string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            var scenario = await _dataServices.LoadScenario(path);

            _availableDiseaseAbilities = scenario.DiseaseAbilities;
            _availableRegionAbilities = scenario.RegionAbilities;
            _interaction = scenario.Interactions;

            _simulationServices.SetRegionAbilities(scenario.RegionAbilities);
            _simulationServices.SetRegions(scenario.Regions.Values.ToList());
            _simulationServices.SetInteractions(scenario.Interactions);
        }

        public async Task SaveSimulationAsync(LogList logs)
        {
            var record = new SimulationRecord
            {
                CreatedAt = DateTime.Now,
                DiseaseName = _simulationServices.GetDiseaseName(),
                DefaultSpreadingSpeed = _simulationServices.GetDiseaseDefaultSpeed(),
                DefaultDeathProbability = _simulationServices.GetDiseaseDefaultDeath(),
                SicknessLength = _simulationServices.GetDiseaseSicknessLength(),
                ImmunityLength = _simulationServices.GetDiseaseImmunityLength()
            };

            var entries = logs.Select(l => new LogEntry
            {
                Day = l.SimulationDate.ToString(),
                Content = l.ToString()
            }).ToList();

            await _dataServices.SaveSimulationAsync(record, entries);
        }

        public async Task<List<SimulationRecord>> GetAllSimulationsAsync() => await _dataServices.GetAllSimulationsAsync();

        public async Task<List<LogEntry>> GetLogsAsync(int simulationId)
            => await _dataServices.GetLogs(simulationId);

        public void EnsureDatabase() => _dataServices.EnsureDatabase();


        // GETTING METHODS

        public Dictionary<int, DiseaseAbility> GetAvailableDiseaseAbilities() => _availableDiseaseAbilities;

        public Dictionary<int, RegionAbility> GetAvailableRegionAbilities() => _availableRegionAbilities;

        public Dictionary<int, Region> GetAllRegions() => _simulationServices.GetAllRegions();

        public string GetRegionString(string input) => _simulationServices.GetRegionString(input);

        public Region GetRegion(int regionId) => _simulationServices.GetRegion(regionId);

        public string GetDiseaseName() => _simulationServices.GetDiseaseName();

        public double GetDiseaseDefaultSpeed() => _simulationServices.GetDiseaseDefaultSpeed();

        public double GetDiseaseTotalSpeed() => _simulationServices.GetDiseaseTotalSpeed();

        public double GetDiseaseDefaultDeath() => _simulationServices.GetDiseaseDefaultDeath();

        public double GetDiseaseTotalDeath() => _simulationServices.GetDiseaseTotalDeath();

        public List<DiseaseAbility> GetActiveDiseaseAbilities() => _simulationServices.GetActiveDiseaseAbilities();

        public (double, double) GetVaccineParameters() => _simulationServices.GetVaccineParameters();

        public LogList GetLogs() => _simulationServices.GetLogs();

        public DateOnly GetDate() => _simulationServices.GetDate();

        // SETTING METHODS

        public void SetSimulation() => _simulationServices.SetSimulation();

        public void SetDisease(string name, double speed, double deathProbability, int length, int immunityLength) => _simulationServices.SetDisease(name, speed, deathProbability, length, immunityLength);

        public void SetStartingRegion(string? input) => _simulationServices.SetStartingRegion(input);

        public void SetVaccine(double protectionEfficiency, double deathProtectionEfficiency) => _simulationServices.SetVaccine(protectionEfficiency, deathProtectionEfficiency);


        // CONTROLLING METHODS
        public void ChangeRegionHealthcareIndex(int regionId, string value) => _simulationServices.ChangeRegionHealthcareIndex(regionId, value);

        public void AddRegionAbility(int regionId, RegionAbility abiltiy) => _simulationServices.AddRegionAbility(regionId, abiltiy);

        public void RemoveRegionAbility(int regionId, RegionAbility abiltiy) => _simulationServices.RemoveRegionAbility(regionId, abiltiy);

        public void ChangeDefaultSpreadingSpeed(string? input) => _simulationServices.ChangeDefaultSpreadingSpeed(input);

        public void ChangeDeathProbability(string? input) => _simulationServices.ChangeDeathProbability(input);

        public void AddDiseaseAbilityToDisease(int id)
        {
            if (!_availableDiseaseAbilities.TryGetValue(id, out DiseaseAbility? ability))
                throw new ArgumentException($"Ability s id {id} neexistuje.");

            _simulationServices.AddDiseaseAbility(ability);
        }

        public void RemoveDiseaseAbilityFromDisease(int id)
        {
            if (!_availableDiseaseAbilities.TryGetValue(id, out DiseaseAbility? ability))
                throw new ArgumentException($"Ability s id {id} neexistuje.");

            _simulationServices.RemoveDiseaseAbility(ability);
        }

        public void ChangeVaccineEfficiency(double? protectionEfficiency, double? deathProtectionEfficiency) => _simulationServices.ChangeVaccineEfficiency(protectionEfficiency, deathProtectionEfficiency);

        public void StartVaccinatingAllRegions() => _simulationServices.StartVaccinatingAllRegions();

        public void StopVaccinatingAllRegions() => _simulationServices?.StopVaccinatingAllRegions();

        public void StartVaccinatingSingleRegion(int regionId) => _simulationServices.StartVaccinatingSingleRegion(regionId);

        public void StopVaccinatingSingleRegion(int regionId) => _simulationServices.StopVaccinatingSingleRegion(regionId);

        public void ChangeSimulationSpeed(int speed) => _simulationServices.ChangeSimulationSpeed(speed);

        public void StartSimulation() => _simulationServices.StartSimulation();

        public void StopSimulation() => _simulationServices.StopSimulation();

        
        // UPDATING METHODS
        public void UpdateAllRegions() => _simulationServices.UpdateAllRegions();

        public void UpdateRegionsDiseaseValues() => _simulationServices.UpdateRegionsDiseaseValues();

        public void ProcessPendingCommands() => _simulationServices.ProcessPendingCommands();

    }
}