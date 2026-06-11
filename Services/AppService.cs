using SimulationCore;
using Shared;

namespace Services
{
    /// <summary>
    /// Facade service that composes data and simulation services for the UI.
    /// </summary>
    public class AppService
    {
        /// <summary>
        /// Event forwarded from simulation to notify UI about simulated days.
        /// </summary>
        public event Action<StatisticUpdate>? OnDaySimulated;

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
            _availableDiseaseAbilities = new();
            _availableRegionAbilities = new();
            _interaction = new();
        }

        // --- DataServices ---

        /// <summary>
        /// Loads scenario data from path and populates local caches and simulation regions.
        /// </summary>
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

        // --- Getters ---

        /// <summary>
        /// Returns available disease abilities.
        /// </summary>
        public Dictionary<int, DiseaseAbility> GetAvailableDiseaseAbilities()
        {
            return _availableDiseaseAbilities;
        }

        /// <summary>
        /// Returns available region abilities.
        /// </summary>
        public Dictionary<int, RegionAbility> GetAvailableRegionAbilities()
        {
            return _availableRegionAbilities;
        }

        /// <summary>
        /// Returns all regions from the simulation.
        /// </summary>
        public Dictionary<int, Region> GetAllRegions() => _simulationServices.GetAllRegions();

        /// <summary>
        /// Returns string representation for a region by input id.
        /// </summary>
        public string GetRegionString(string input) => _simulationServices.GetRegionString(input);

        /// <summary>
        /// Returns a region instance by id.
        /// </summary>
        public Region GetRegion(int regionId) => _simulationServices.GetRegion(regionId);

        // --- Setters ---

        /// <summary>
        /// Initializes simulation inside simulation service.
        /// </summary>
        public void SetSimulation() => _simulationServices.SetSimulation();

        /// <summary>
        /// Sets disease in the simulation using explicit parameters.
        /// </summary>
        public void SetDisease(string name, double speed, double deathProbability, int length, int immunityLength) => _simulationServices.SetDisease(name, speed, deathProbability, length, immunityLength);

        /// <summary>
        /// Sets disease in the simulation by id (not implemented).
        /// </summary>
        public void SetDisease(int id) => _simulationServices.SetDisease(id);

        /// <summary>
        /// Sets the starting region by parsing user input.
        /// </summary>
        public void SetStartingRegion(string? input)
        {
            _simulationServices.setStartingRegion(input);
        }


        public void SetStartDate()
        {
            _simulationServices.SetStartDate();
        }

        public DateOnly GetDate() => _simulationServices.GetDate();
        /// <summary>
        /// Sets region spreading speed via simulation service.
        /// </summary>
        public void SetRegionSpreadingSpeed(int regionId, string value) => _simulationServices.SetRegionSpreadingSpeed(regionId, value);

        /// <summary>
        /// Sets region healthcare index via simulation service.
        /// </summary>
        public void SetRegionHealthcareIndex(int regionId, string value) => _simulationServices.SetRegionHealthcareIndex(regionId, value);

        /// <summary>
        /// Adds an ability to a region via simulation service.
        /// </summary>
        public void AddRegionAbility(int regionId, RegionAbility abiltiy) => _simulationServices.AddRegionAbility(regionId, abiltiy);

        /// <summary>
        /// Removes an ability from a region via simulation service.
        /// </summary>
        public void RemoveRegionAbility(int regionId, RegionAbility abiltiy) => _simulationServices.RemoveRegionAbility(regionId, abiltiy);

        /// <summary>
        /// Changes the global default spreading speed (user input).
        /// </summary>
        public void ChangeDefaultSpreadingSpeed(string? input) => _simulationServices.changeDefaultSpreadingSpeed(input);

        /// <summary>
        /// Changes the global death probability (user input).
        /// </summary>
        public void ChangeDeathProbability(string? input) => _simulationServices.changeDeathProbability(input);

        /// <summary>
        /// Adds a disease ability to the current disease by ability id.
        /// </summary>
        public void AddDiseaseAbilityToDisease(int id)
        {
            if (!_availableDiseaseAbilities.TryGetValue(id, out DiseaseAbility? ability))
                throw new ArgumentException($"Ability s id {id} neexistuje.");

            _simulationServices.AddDiseaseAbility(ability);
        }


        public void SetVaccine(double protectionEfficiency, double deathProtectionEfficiency) => _simulationServices.SetVaccine(protectionEfficiency, deathProtectionEfficiency);

        public void ChangeVaccineEfficiency(double? protectionEfficiency, double? deathProtectionEfficiency) => _simulationServices.ChangeVaccineEfficiency(protectionEfficiency, deathProtectionEfficiency);

        public string GetDiseaseName() => _simulationServices.GetDiseaseName();
        public double GetDiseaseDefaultSpeed() => _simulationServices.GetDiseaseDefaultSpeed();

        public double GetDiseaseTotalSpeed() => _simulationServices.GetDiseaseTotalSpeed();

        public double GetDiseaseDefaultDeath() => _simulationServices.GetDiseaseDefaultDeath();

        public double GetDiseaseTotalDeath() => _simulationServices.GetDiseaseTotalDeath();

        public (double, double) GetVaccineParameters() => _simulationServices.GetVaccineParameters();



        public void StartVaccinatingAllRegions() => _simulationServices.StartVaccinatingAllRegions();


        public void StopVaccinatingAllRegions() => _simulationServices?.StopVaccinatingAllRegions();

        public void StartVaccinatingSingleRegion(int regionId) => _simulationServices.StartVaccinatingSingleRegion(regionId);

        public void StopVaccinatingSingleRegion(int regionId) => _simulationServices.StopVaccinatingSingleRegion(regionId);

        public void ChangeSimulationSpeed(int speed) => _simulationServices.ChangeSimulationSpeed(speed);

        /// <summary>
        /// Removes a disease ability from the current disease by ability id.
        /// </summary>
        public void RemoveDiseaseAbilityFromDisease(int id)
        {
            if (!_availableDiseaseAbilities.TryGetValue(id, out DiseaseAbility? ability))
                throw new ArgumentException($"Ability s id {id} neexistuje.");

            _simulationServices.RemoveDiseaseAbility(ability);
        }

        // --- Simulation control ---

        /// <summary>
        /// Starts the simulation run.
        /// </summary>
        public void StartSimulation() => _simulationServices.startSimulation();

        /// <summary>
        /// Stops the simulation run.
        /// </summary>
        public void StopSimulation() => _simulationServices.stopSimulation();
    }
}