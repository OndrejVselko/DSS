using System;
using SimulationCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SimulationServices
    {
        public event Action<string>? OnDaySimulated;
        Simulation simulation { get; set; }
        private CancellationTokenSource? _cts;
        public SimulationServices() { }

        /* Setting methods */
        public void SetSimulation()
        {
            simulation = new Simulation();
            simulation.OnDaySimulated += (msg) => OnDaySimulated?.Invoke(msg);
        }

        public void SetDisease(int id)
        {
            throw new NotImplementedException();
        }

        public void SetDisease(string name, double speed, double deathProbability, int length)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Název nemoci nesmí být prázdný.");
            }

            if (speed <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(speed), "Rychlost šíření musí být kladné číslo.");
            }

            if (deathProbability < 0 || deathProbability > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(deathProbability), "Pravděpodobnost smrti musí být v rozmezí 0 až 1 (včetně).");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Délka trvání nemoci musí být alespoň 1 den.");
            }

            simulation.SetDisease(new Disease(name, speed, deathProbability, length));
            SetRegionsQueues();
            UpdateRegionsDiseaseValues();
        }

        public void SetRegions(List<Region> regions) => simulation.SetRegions(regions);

        public void SetRegionsQueues() => simulation.SetRegionQueues();

        public void UpdateRegionsDiseaseValues() => simulation.UpdateRegionsDiseaseValues();

        public void SetStartDate(DateOnly startDate = default) => simulation.SetStartDate(startDate);

        public void startSimulation()
        {
            if (_cts != null) return;

            simulation.Run(); 
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => simulation.Simulate(_cts.Token));
        }

        public void stopSimulation()
        {
            simulation.Stop();
            _cts?.Cancel();
            _cts = null;
        }

        public void setStartingRegion(string? input)
        {
            if (!int.TryParse(input, out int regionId))
                throw new ArgumentException("Zadejte číslo.");

            if (!simulation.regions.ContainsKey(regionId))
                throw new ArgumentException($"Region s id {regionId} neexistuje.");

            simulation.regions[regionId].Sick += 1;
        }

        public void changeDefaultSpreadingSpeed(string? input)
        {
            if (input == null || !double.TryParse(input, out double defaultSpreadingSpeed))
                throw new ArgumentException("Zadejte číselnou hodnotu");
            simulation.userActions.Enqueue(new UserAction(UserAction.ActionType.ChangeDefaultSpreadingSpeed, defaultSpreadingSpeed));
        }

        public void changeDeathProbability(string? input)
        {
            if (input == null || !double.TryParse(input, out double deathProbability) || !(deathProbability >= 0 && deathProbability <= 1))
                throw new ArgumentException("Zadejte číselnou hodnotu");
            simulation.userActions.Enqueue(new UserAction(UserAction.ActionType.ChangeDeathProbability, deathProbability));
        }

        public void AddDiseaseAbility(DiseaseAbility ability)
        {
            simulation.userActions.Enqueue(new UserAction(UserAction.ActionType.AddDiseaseAbility, ability: ability));
        }

        public void RemoveDiseaseAbility(DiseaseAbility ability)
        {
            simulation.userActions.Enqueue(new UserAction(UserAction.ActionType.RemoveDiseaseAbility, ability: ability));
        }

        public Dictionary<int, Region> GetAllRegions()
        {
            return simulation.regions;
        }
    }
}
