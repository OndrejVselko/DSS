using System.Globalization;

namespace SimulationCore
{
    /// <summary>
    /// Core simulation orchestration: regions, disease, commands and day loop.
    /// </summary>
    public class Simulation
    {
        /// <summary>
        /// Event invoked after each simulated day with a textual report.
        /// </summary>
        public event Action<string>? OnDaySimulated;

        /// <summary>Disease under simulation.</summary>
        public Disease disease { get; set; }
        /// <summary>Dictionary of regions keyed by id.</summary>
        public Dictionary<int, Region> regions { get; set; }

        public Vaccine vaccine { get; set; }


        /// <summary>Simulation start date.</summary>
        public DateOnly startDate { get; set; }
        /// <summary>Current simulation date.</summary>
        public DateOnly currentSimulationDate { get; set; }
        private bool _isRunning;
        /// <summary>Interval in days for reporting (unused currently).</summary>
        public int reportingInterval;
        /// <summary>Milliseconds per simulated day.</summary>
        public int dayLength { get; set; }

        /// <summary>Queue of pending simulation commands to execute before each day.</summary>
        public Queue<ISimulationCommand> pendingCommands;


        public int GlobalPopulation;
        /// <summary>
        /// Initializes default parameters and command queue.
        /// </summary>
        public Simulation()
        {
            this.pendingCommands = new Queue<ISimulationCommand>();
            this.reportingInterval = 1;
            this.dayLength = 1000;
        }

        /// <summary>
        /// Enqueues a simulation command to be applied before the next simulated day.
        /// </summary>
        public void EnqueueCommand(ISimulationCommand command)
        {
            pendingCommands.Enqueue(command);
        }

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

            GlobalPopulation = regions.Sum(x => x.Population);
        }

        public void AssignNeighbours()
        {
            foreach (var region in regions.Values)
                foreach (var id in region.NeighbourIds)
                    if (regions.TryGetValue(id, out var neighbour))
                        region.NeighbouringRegions.Add(neighbour);
        }

        public void SetVaccine(Vaccine vaccine)
        {
            this.vaccine = vaccine;
            foreach (int key in this.regions.Keys)
            {
                regions[key].SetVaccine(vaccine);
            }
        }

        /// <summary>
        /// Sets the simulation start date and current date when default requested.
        /// </summary>
        public void SetStartDate(DateOnly startDate = default)
        {
            if (startDate == default)
            {
                this.startDate = DateOnly.FromDateTime(DateTime.Now);
                this.currentSimulationDate = DateOnly.FromDateTime(DateTime.Now);
            }
        }

        /// <summary>Marks the simulation as running.</summary>
        public void Run() => this._isRunning = true;

        /// <summary>Marks the simulation as stopped.</summary>
        public void Stop() => this._isRunning = false;

        /// <summary>Indicates whether the simulation is running.</summary>
        public bool IsRunning() => this._isRunning;

        /// <summary>Prepares region queues using disease sickness length.</summary>
        public void SetRegionQueues()
        {
            foreach (int key in this.regions.Keys)
                regions[key].SetStartingQueue(this.disease.SicknessLength);
        }

        /// <summary>Updates disease values in all regions.</summary>
        public void UpdateRegionsDiseaseValues()
        {
            foreach (var key in this.regions.Keys)
                regions[key].UpdateDiseaseValues(disease.TotalSpreadingSpeed, disease.DeathProbability);
        }

        /// <summary>Updates both disease and region-derived values for all regions.</summary>
        public void UpdateAllRegions()
        {
            foreach (var key in regions.Keys)
            {
                regions[key].UpdateDiseaseValues(disease.TotalSpreadingSpeed, disease.DeathProbability);
                regions[key].UpdateRegionValues();
            }
        }

        /// <summary>Changes simulated day length (ms) with validation.</summary>
        public void changeDayLength(int ms)
        {
            if (ms > 0)
                this.dayLength = ms;
            else
                Console.WriteLine("Neplatný čas");
        }

        /// <summary>
        /// Background simulation loop advancing dates and invoking OnDaySimulated.
        /// </summary>
        public async Task Simulate(CancellationToken ct)
        {
            while (this._isRunning && !ct.IsCancellationRequested)
            {
                currentSimulationDate = currentSimulationDate.AddDays(1);
                string dayString = currentSimulationDate.ToString() + "\n" + SimulateDay().ToString();
                OnDaySimulated?.Invoke(dayString);

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
            {
                regions[key].RecalculateRandomOccurrence(totalSick, GlobalPopulation);
            }

                return new StatisticUpdate(newSick, newDead, newVaccinated, totalSick, totalDeath, totalVaccinated);
        }
    }
}