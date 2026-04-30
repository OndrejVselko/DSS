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
        public SimulationServices() { 
        }

        public void setSimulation(Disease disease, List<Region> regions)
        {
            simulation = new Simulation(disease, regions);
            simulation.OnDaySimulated += (msg) => OnDaySimulated?.Invoke(msg);
        }

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

            simulation.regions[regionId].sick += 1;
        }

        public void changeDefaultSpreadingSpeed(string? input)
        {
            if (input == null || !double.TryParse(input, out double defaultSpreadingSpeed))
                throw new ArgumentException("Zadejte číselnou hodnotu");
            simulation.userActions.Enqueue(new UserAction(UserAction.ActionType.ChangeDefaultSpreadingSpeed, defaultSpreadingSpeed));
           

        }
    }
}
