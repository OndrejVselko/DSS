using SimulationCore;

namespace Services
{
    public class AppService
    {
        private readonly SimulationServices _simulationServices;
        private readonly DataServices _dataServices;
        private Dictionary<int, DiseaseAbility> _availableDiseaseAbilities;
        private Dictionary<int, RegionAbility> _availableRegionAbilities;
        private Dictionary<(int , int), Interaction> _interaction;

        public event Action<string>? OnDaySimulated;

        public AppService()
        {
            _simulationServices = new SimulationServices();
            _dataServices = new DataServices();
            _simulationServices.OnDaySimulated += msg => OnDaySimulated?.Invoke(msg);
            _availableDiseaseAbilities = new();
            _availableRegionAbilities = new();
            _interaction = new();
        }

        // --- DataServices ---

        public async Task LoadData(string path)
        {
            if (path == null) throw new ArgumentNullException("input");

            var scenario = await _dataServices.LoadScenario(path);

            _availableDiseaseAbilities = scenario.DiseaseAbilities;
            _availableRegionAbilities = scenario.RegionAbilities;
            _interaction = scenario.Interactions;

            _simulationServices.SetRegions(scenario.Regions.Values.ToList());
        }
        public Dictionary<int, DiseaseAbility> GetAvailableDiseaseAbilities()
        {
            return _availableDiseaseAbilities;
        }
        public Dictionary<int, RegionAbility> GetAvailableRegionAbilities()
        {
            return _availableRegionAbilities;
        }
        public Dictionary<int, Region> GetAllRegions() => _simulationServices.GetAllRegions();
        public string GetRegionString(string input) => _simulationServices.GetRegionString(input);
        public Region GetRegion(int regionId) => _simulationServices.GetRegion(regionId);


        // --- Sestavení simulace ---
        public void SetSimulation() => _simulationServices.SetSimulation();
        public void SetDisease(string name, double speed, double deathProbability, int length) => _simulationServices.SetDisease(name, speed, deathProbability, length);
        public void SetDisease(int id) => _simulationServices.SetDisease(id);
        public void SetStartingRegion(string? input)
        {
            _simulationServices.setStartingRegion(input);
        }

        public void SetRegionSpreadingSpeed(int regionId, string value) => _simulationServices.SetRegionSpreadingSpeed(regionId, value);
        public void SetRegionHealthcareIndex(int regionId, string value) => _simulationServices.SetRegionHealthcareIndex(regionId, value);
        public void AddRegionAbility(int regionId, RegionAbility abiltiy) => _simulationServices.AddRegionAbility(regionId, abiltiy);
        public void RemoveRegionAbility(int regionId, RegionAbility abiltiy) => _simulationServices.RemoveRegionAbility(regionId, abiltiy);

        // --- Ovládání simulace ---

        public void StartSimulation() => _simulationServices.startSimulation();
        public void StopSimulation() => _simulationServices.stopSimulation();
        public void ChangeDefaultSpreadingSpeed(string? input) => _simulationServices.changeDefaultSpreadingSpeed(input);
        public void ChangeDeathProbability(string? input) => _simulationServices.changeDeathProbability(input);
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
    }
}