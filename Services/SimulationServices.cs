using System;
using SimulationCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Wraps the core Simulation and exposes control methods for the UI.
    /// </summary>
    public class SimulationServices
    {
        /// <summary>
        /// Event raised when a day is simulated (for UI output).
        /// </summary>
        public event Action<string>? OnDaySimulated;

        /// <summary>
        /// Internal simulation instance.
        /// </summary>
        Simulation simulation { get; set; }

        /// <summary>
        /// Cancellation token source used to stop background simulation task.
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SimulationServices() { }

        /// <summary>
        /// Initializes a new Simulation and subscribes to its day event.
        /// </summary>
        public void SetSimulation()
        {
            simulation = new Simulation();
            simulation.OnDaySimulated += (msg) => OnDaySimulated?.Invoke(msg);
        }

        /// <summary>
        /// Sets disease by id (not implemented).
        /// </summary>
        public void SetDisease(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and sets a disease in the simulation with validation.
        /// </summary>
        public void SetDisease(string name, double speed, double deathProbability, int length)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název nemoci nesmí být prázdný.");

            if (speed <= 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Rychlost šíření musí být kladné číslo.");

            if (deathProbability < 0 || deathProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(deathProbability), "Pravděpodobnost smrti musí být v rozmezí 0 až 1 (včetně).");

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Délka trvání nemoci musí být alespoň 1 den.");

            simulation.SetDisease(new Disease(name, speed, deathProbability, length));
            SetRegionsQueues();
            UpdateRegionsDiseaseValues();
        }

        /// <summary>
        /// Sets regions in the simulation.
        /// </summary>
        public void SetRegions(List<Region> regions) => simulation.SetRegions(regions);

        /// <summary>
        /// Prepares region queues inside simulation.
        /// </summary>
        public void SetRegionsQueues() => simulation.SetRegionQueues();

        /// <summary>
        /// Updates region-level disease-related values.
        /// </summary>
        public void UpdateRegionsDiseaseValues() => simulation.UpdateRegionsDiseaseValues();

        /// <summary>
        /// Sets the simulation start date.
        /// </summary>
        public void SetStartDate(DateOnly startDate = default) => simulation.SetStartDate(startDate);

        public DateOnly GetDate()
        {
            return simulation.currentSimulationDate;
        }

        /// <summary>
        /// Starts the simulation loop in a background task.
        /// </summary>
        public void startSimulation()
        {
            if (_cts != null) return;
            simulation.Run();
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => simulation.Simulate(_cts.Token));
        }


        public void SetRegionAbilities(Dictionary<int, RegionAbility> regionAbilities) => simulation.SetRegionAbilities(regionAbilities);

        /// <summary>
        /// Stops the running simulation and cancels the background task.
        /// </summary>
        public void stopSimulation()
        {
            simulation.Stop();
            _cts?.Cancel();
            _cts = null;
        }

        /// <summary>
        /// Parses user input and sets the starting region by id.
        /// </summary>
        public void setStartingRegion(string? input)
        {
            if (!int.TryParse(input, out int regionId))
                throw new ArgumentException("Zadejte číslo.");

            if (!simulation.regions.ContainsKey(regionId))
                throw new ArgumentException($"Region s id {regionId} neexistuje.");

            simulation.regions[regionId].Sick += 1;
        }

        // --- Prikazy pro nemoc ---

        /// <summary>
        /// Parses input and enqueues a command to change default spreading speed.
        /// </summary>
        public void changeDefaultSpreadingSpeed(string? input)
        {
            if (input == null || !double.TryParse(input, out double defaultSpreadingSpeed))
                throw new ArgumentException("Zadejte číselnou hodnotu.");
            simulation.EnqueueCommand(new ChangeDefaultSpreadingSpeedCommand(defaultSpreadingSpeed));
        }

        /// <summary>
        /// Parses input and enqueues a command to change death probability.
        /// </summary>
        public void changeDeathProbability(string? input)
        {
            if (input == null || !double.TryParse(input, out double deathProbability) || !(deathProbability >= 0 && deathProbability <= 1))
                throw new ArgumentException("Zadejte číselnou hodnotu.");
            simulation.EnqueueCommand(new ChangeDeathProbabilityCommand(deathProbability));
        }

        /// <summary>
        /// Enqueues a command to add a disease ability to the current disease.
        /// </summary>
        public void AddDiseaseAbility(DiseaseAbility ability)
            => simulation.EnqueueCommand(new AddDiseaseAbilityCommand(ability));

        /// <summary>
        /// Enqueues a command to remove a disease ability from the current disease.
        /// </summary>
        public void RemoveDiseaseAbility(DiseaseAbility ability)
            => simulation.EnqueueCommand(new RemoveDiseaseAbilityCommand(ability));

        // --- Prikazy pro regiony ---

        /// <summary>
        /// Validates and enqueues a command to change a region's spreading speed.
        /// </summary>
        public void SetRegionSpreadingSpeed(int regionId, string value)
        {
            Region region = GetRegion(regionId);
            if (!double.TryParse(value, out double newSpreadingSpeed) || newSpreadingSpeed < 0)
                throw new ArgumentException("Neplatná hodnota.");
            simulation.EnqueueCommand(new ChangeRegionSpreadingSpeedCommand(region, newSpreadingSpeed));
        }

        /// <summary>
        /// Validates and enqueues a command to change a region's healthcare index.
        /// </summary>
        public void SetRegionHealthcareIndex(int regionId, string value)
        {
            Region region = GetRegion(regionId);
            if (!double.TryParse(value, out double newHealthcareIndex) || newHealthcareIndex < 0)
                throw new ArgumentException("Neplatná hodnota.");
            simulation.EnqueueCommand(new ChangeRegionHealthcareIndexCommand(region, newHealthcareIndex));
        }

        public void SetInteractions(Dictionary<(int, int), Interaction> interactions) => simulation.SetInteractions(interactions);

        /// <summary>
        /// Enqueues a command to add an ability to a region.
        /// </summary>
        public void AddRegionAbility(int regionId, RegionAbility ability)
            => simulation.EnqueueCommand(new AddRegionAbilityCommand(GetRegion(regionId), ability));

        /// <summary>
        /// Enqueues a command to remove an ability from a region.
        /// </summary>
        public void RemoveRegionAbility(int regionId, RegionAbility ability)
            => simulation.EnqueueCommand(new RemoveRegionAbilityCommand(GetRegion(regionId), ability));

        // --- Prikazy pro ockovani

        public void SetVaccine(double protectionEfficiency, double deathProtectionEfficiency)
        {
            if (protectionEfficiency >= 0 && protectionEfficiency <= 1 && deathProtectionEfficiency >= 0 && deathProtectionEfficiency <= 1)
            {
                Vaccine vaccine = new Vaccine(protectionEfficiency, deathProtectionEfficiency);
                simulation.SetVaccine(vaccine);
            }
            else
            {
                throw new ArgumentException("Hodnoty jsou mimo rozsah 0-1");
            }
        }


        public void ChangeVaccineEfficiency(double? protectionEfficiency, double? deathProtectionEfficiency)
        {
            if (protectionEfficiency.HasValue)
            {
                if (!(protectionEfficiency >= 0 && protectionEfficiency <= 1))
                {
                    throw new ArgumentException("Hodnoty jsou mimo rozsah 0-1");
                }
            }

            if (deathProtectionEfficiency.HasValue)
            {
                if (!(deathProtectionEfficiency >= 0 && deathProtectionEfficiency <= 1))
                {
                    throw new ArgumentException("Hodnoty jsou mimo rozsah 0-1");
                }
            }

            simulation.EnqueueCommand(new ChangeVaccineParametersCommand(protectionEfficiency, deathProtectionEfficiency));
        }

        public void StartVaccinatingAllRegions()
        {
            if (simulation.vaccine is null)
            {
                throw new Exception("Neexistuje vakcína, nejprve ji vytvořte");
            }

            simulation.EnqueueCommand(new StartVaccinationAllRegionCommand());
        }

        public void StopVaccinatingAllRegions()
        {
            simulation.EnqueueCommand(new StopVaccinationAllRegionCommand());
        }

        public void StartVaccinatingSingleRegion(int regionId)
            => simulation.EnqueueCommand(new StartVaccinationSingleRegionCommand(GetRegion(regionId)));

        public void StopVaccinatingSingleRegion(int regionId)
            => simulation.EnqueueCommand(new StopVaccinationSingleRegionCommand(GetRegion(regionId)));


        // --- Dotazy ---

        /// <summary>
        /// Returns the internal regions dictionary.
        /// </summary>
        public Dictionary<int, Region> GetAllRegions() => simulation.regions;

        /// <summary>
        /// Returns a string description of a region by input id or throws.
        /// </summary>
        public string GetRegionString(string input)
        {
            if (int.TryParse(input, out int regionIndex) && simulation.regions.ContainsKey(regionIndex))
                return simulation.regions[regionIndex].ToString();
            throw new ArgumentException("Neplatné id.");
        }

        /// <summary>
        /// Returns a specific region or throws when id is invalid.
        /// </summary>
        public Region GetRegion(int regionId)
        {
            if (simulation.regions.ContainsKey(regionId))
                return simulation.regions[regionId];
            throw new ArgumentException("Neplatné id regionu."); // Nikdy by nemělo nastat
        }

        public string GetDiseaseName()
        {
            if(simulation.disease is not null)
                return simulation.disease.Name;

            return "";
        }


        public double GetDiseaseDefaultSpeed()
        {
            if (simulation.disease is not null)
                return simulation.disease.DefaultSpreadingSpeed;

            return double.NaN;
        }

        public double GetDiseaseTotalSpeed(){
            if (simulation.disease is not null)
                return simulation.disease.TotalSpreadingSpeed;

            return double.NaN;
        }

        public double GetDiseaseDefaultDeath()
        {
            if (simulation.disease is not null)
                return simulation.disease.DefaultDeathProbability;

            return double.NaN;
        }

        public double GetDiseaseTotalDeath() {
            if (simulation.disease is not null)
                return simulation.disease.TotalDeathProbability;

            return double.NaN;
        }
    }
}