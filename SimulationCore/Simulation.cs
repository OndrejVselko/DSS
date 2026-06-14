using System.Globalization;
using Shared;
namespace SimulationCore
{
    /// <summary>
    /// Core simulation orchestration: regions, disease, commands and day loop.
    /// </summary>
    public class Simulation
    {

        public Disease disease { get; set; }
        public Dictionary<int, Region> regions { get; set; }
        public Vaccine vaccine { get; set; }

        public DateOnly startDate { get; set; }
        public DateOnly currentSimulationDate { get; set; }
        private bool _isRunning;
        public int dayLength { get; set; }
        public Queue<ISimulationCommand> pendingCommands;
        public Dictionary<(int, int), Interaction> Interactions { get; set; } = new();
        private Dictionary<int, RegionAbility> _regionAbilities = new();
        public long GlobalPopulation;
        public LogList Logs { get; set; } = new LogList();

        public event Action<StatisticUpdate>? OnDaySimulated;

        public Simulation()
        {
            this.pendingCommands = new Queue<ISimulationCommand>();
            this.dayLength = 1000;
            this.startDate = DateOnly.FromDateTime(DateTime.Now);
            this.currentSimulationDate = DateOnly.FromDateTime(DateTime.Now);
        }

        // SETTING METHODS

        /// <summary>
        /// Sets the active disease instance.
        /// </summary>
        public void SetDisease(Disease disease)
        {
            this.disease = disease;
        }

        /// <summary>
        /// Initializes internal regions dictionary from a list.
        /// </summary>
        public void SetRegions(List<Region> regions)
        {
            this.regions = new Dictionary<int, Region>();
            foreach (var region in regions)
                this.regions[region.Id] = region;

            AssignNeighbours();
            AssignAbilities();

            GlobalPopulation = regions.Sum(x => x.Population);
        }

        public void SetRegionAbilities(Dictionary<int, RegionAbility> regionAbilities)
        {
            _regionAbilities = regionAbilities;
        }

        /// <summary>Prepares region queues using disease sickness length.</summary>
        public void SetRegionQueues()
        {
            foreach (int key in this.regions.Keys)
                regions[key].SetStartingQueue(disease.SicknessLength, disease.ImmunityLength);
        }

        public void AssignNeighbours()
        {
            foreach (var region in regions.Values)
                foreach (var id in region.NeighbourIds)
                    if (regions.TryGetValue(id, out var neighbour))
                        region.NeighbouringRegions.Add(neighbour);
        }

        private void AssignAbilities()
        {
            foreach (var region in regions.Values)
                foreach (var id in region.AbilityIds)
                    if (_regionAbilities.TryGetValue(id, out var ability))
                        region.AddAbility(ability);
        }

        public void AssignRegionAbilities(Dictionary<int, RegionAbility> regionAbilities)
        {
            foreach (var region in regions.Values)
                foreach (var id in region.AbilityIds)
                    if (regionAbilities.TryGetValue(id, out var ability))
                        region.AddAbility(ability);
        }

        public void SetVaccine(Vaccine vaccine)
        {
            this.vaccine = vaccine;
            foreach (int key in this.regions.Keys)
            {
                regions[key].SetVaccine(vaccine);
            }
        }

        public void SetInteractions(Dictionary<(int, int), Interaction> interactions)
        {
            Interactions = interactions;
        }


        // CONTROLING METHODS

        /// <summary>Marks the simulation as running.</summary>
        public void Run() => this._isRunning = true;

        /// <summary>Marks the simulation as stopped.</summary>
        public void Stop() => this._isRunning = false;


        /// <summary>Changes simulated day length (ms) with validation.</summary>
        public void changeDayLength(int ms)
        {
            if (ms > 0)
                this.dayLength = ms;
            else
                Console.WriteLine("Neplatný čas");
        }

        public void AddLog(DateOnly date, string message, params string[] args)
        {
            Logs.Add(date, message, args);
        }
        public void EnqueueCommand(ISimulationCommand command)
        {
            pendingCommands.Enqueue(command);
        }



        // UPDATING METHODS
        /// <summary>Updates disease values in all regions.</summary>
        public void UpdateRegionsDiseaseValues()
        {
            foreach (var key in this.regions.Keys)
                regions[key].UpdateDiseaseValues(disease.TotalSpreadingSpeed, disease.TotalDeathProbability);
        }

        /// <summary>Updates both disease and region-derived values for all regions.</summary>
        public void UpdateAllRegions()
        {
            foreach (var key in regions.Keys)
            {
                regions[key].UpdateDiseaseValues(disease.TotalSpreadingSpeed, disease.TotalDeathProbability);
                regions[key].UpdateRegionValues();
            }
        }


        // SIMULATION METHODS

        /// <summary>
        /// Background simulation loop advancing dates and invoking OnDaySimulated.
        /// </summary>
        public async Task Simulate(CancellationToken ct)
        {
            while (this._isRunning && !ct.IsCancellationRequested)
            {
                currentSimulationDate = currentSimulationDate.AddDays(1);
                var update = SimulateDay();
                OnDaySimulated?.Invoke(update);

                try { await Task.Delay(dayLength, ct); }
                catch (TaskCanceledException) { break; }
            }
        }

        /// <summary>
        /// Executes pending commands then simulates one day across all regions and aggregates stats.
        /// </summary>
        private StatisticUpdate SimulateDay()
        {
            while (pendingCommands.Count > 0)
                pendingCommands.Dequeue().Execute(this);

            int totalSick = 0, totalDeath = 0, totalVaccinated = 0;
            int newSick = 0, newDead = 0, newVaccinated = 0;

            foreach (int key in this.regions.Keys)
            {
                StatisticUpdate regionUpdate = regions[key].SimulateDay();
                newDead += regionUpdate.NewDead;
                newSick += regionUpdate.NewSick;
                newVaccinated += regionUpdate.NewVaccinated;
                totalSick += regionUpdate.TotalSick;
                totalDeath += regionUpdate.TotalDead;
                totalVaccinated += regionUpdate.TotalVaccinated;
            }

            foreach (int key in this.regions.Keys)
                regions[key].RecalculateRandomOccurrence(totalSick, GlobalPopulation);

            var regionsByIso = regions.Values
                .Where(r => !string.IsNullOrEmpty(r.IsoCode))
                .ToDictionary(r => r.IsoCode, r => r);

            StatisticUpdate update = new StatisticUpdate(currentSimulationDate, newSick, newDead, newVaccinated,
                totalSick, totalDeath, totalVaccinated, regionsByIso);

            AddLog(currentSimulationDate, update.ToString());

            return update;
        }
    }
}