using SimulationCore;

namespace Services
{
    public class AppService
    {
        private readonly SimulationServices _simulationServices;
        private readonly DataServices _dataServices;
        private Dictionary<int, DiseaseAbility> _availableDiseaseAbilities;

        public event Action<string>? OnDaySimulated;

        public AppService()
        {
            _simulationServices = new SimulationServices();
            _dataServices = new DataServices();
            _simulationServices.OnDaySimulated += msg => OnDaySimulated?.Invoke(msg);
            _availableDiseaseAbilities = new();
        }

        // --- DataServices ---

        public async Task<List<Region>> LoadRegionsFromJson(string path)
        {
            var regions = await _dataServices.LoadRegionsFromJson(path);
            _simulationServices.SetRegions(regions);
            return regions;
        }

        public async Task<Dictionary<int, DiseaseAbility>> LoadDiseaseAbilities(string path)
        {
            _availableDiseaseAbilities = await _dataServices.LoadDiseaseAbilities(path);
            return _availableDiseaseAbilities;
        }

        public Dictionary<int, DiseaseAbility> GetAvailableDiseaseAbilities()
        {
            return _availableDiseaseAbilities;
        }

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

        // --- Sestavení simulace ---
        public void SetSimulation() => _simulationServices.SetSimulation();
        public void SetDisease(string name, double speed, double deathProbability, int length) => _simulationServices.SetDisease(name, speed, deathProbability, length);
        public void SetDisease(int id) => _simulationServices.SetDisease(id);

        public void SetStartingRegion(string? input)
        {
            _simulationServices.setStartingRegion(input);
        }

        // --- Ovládání simulace ---

        public void StartSimulation() => _simulationServices.startSimulation();
        public void StopSimulation() => _simulationServices.stopSimulation();
        public void ChangeDefaultSpreadingSpeed(string? input) => _simulationServices.changeDefaultSpreadingSpeed(input);
        public void ChangeDeathProbability(string? input) => _simulationServices.changeDeathProbability(input);
    }
}