using System;
using SimulationCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

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
        public event Action<StatisticUpdate>? OnDaySimulated;
        public event Action<Log>? OnLogAdded;

        /// <summary>
        /// Internal simulation instance.
        /// </summary>
        private Simulation _simulation { get; set; }

        /// <summary>
        /// Cancellation token source used to stop background simulation task.
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SimulationServices() { }


        // UPDATING METHODS 

        public void UpdateRegionsDiseaseValues() => _simulation.UpdateRegionsDiseaseValues();

        public void UpdateAllRegions() => _simulation.UpdateAllRegions();


        // GETTING METHODS

        public DateOnly GetDate()
        {
            return _simulation.CurrentSimulationDate;
        }

        public Dictionary<int, Region> GetAllRegions() => _simulation.Regions;

        public string GetRegionString(string input)
        {
            if (int.TryParse(input, out int regionIndex) && _simulation.Regions.ContainsKey(regionIndex))
                return _simulation.Regions[regionIndex].ToString();
            throw new ArgumentException("Neplatné id.");
        }

        public Region GetRegion(int regionId)
        {
            if (_simulation.Regions.ContainsKey(regionId))
                return _simulation.Regions[regionId];
            throw new ArgumentException("Neplatné id regionu."); // Nikdy by nemělo nastat
        }

        public string GetDiseaseName()
        {
            if(_simulation.Disease is not null)
                return _simulation.Disease.Name;

            return "";
        }

        public double GetDiseaseDefaultSpeed()
        {
            if (_simulation.Disease is not null)
                return _simulation.Disease.DefaultSpreadingSpeed;

            return double.NaN;
        }

        public double GetDiseaseTotalSpeed(){
            if (_simulation.Disease is not null)
                return _simulation.Disease.TotalSpreadingSpeed;

            return double.NaN;
        }

        public double GetDiseaseDefaultDeath()
        {
            if (_simulation.Disease is not null)
                return _simulation.Disease.DefaultDeathProbability;

            return double.NaN;
        }

        public double GetDiseaseTotalDeath()
        {
            if (_simulation.Disease is not null)
                return _simulation.Disease.TotalDeathProbability;

            return double.NaN;
        }

        public List<DiseaseAbility> GetActiveDiseaseAbilities() => _simulation.Disease.Abilities;

        public (double, double) GetVaccineParameters()
        {
            return (_simulation.Vaccine.ProtectionEfficiency, _simulation.Vaccine.DeathProtectionEfficiency);
        }

        public int GetDiseaseSicknessLength()
        {
            return _simulation.Disease.SicknessLength;
        }

        public int GetDiseaseImmunityLength()
        {
            return _simulation.Disease.ImmunityLength;
        }

        public LogList GetLogs() => _simulation.Logs;


        // SETTING METHODS

        public void SetVaccine(double protectionEfficiency, double deathProtectionEfficiency)
        {
            if (protectionEfficiency >= 0 && protectionEfficiency <= 1 && deathProtectionEfficiency >= 0 && deathProtectionEfficiency <= 1)
            {
                Vaccine vaccine = new Vaccine(protectionEfficiency, deathProtectionEfficiency);
                _simulation.SetVaccine(vaccine);
            }
            else
            {
                throw new ArgumentException("Hodnoty jsou mimo rozsah 0-1");
            }
        }

        public void SetInteractions(Dictionary<(int, int), Interaction> interactions) => _simulation.SetInteractions(interactions);

        public void SetRegionAbilities(Dictionary<int, RegionAbility> regionAbilities) => _simulation.SetRegionAbilities(regionAbilities);

        public void SetStartingRegion(string? input)
        {
            if (!int.TryParse(input, out int regionId))
                throw new ArgumentException("Zadejte číslo.");

            if (!_simulation.Regions.ContainsKey(regionId))
                throw new ArgumentException($"Region s id {regionId} neexistuje.");

            _simulation.Regions[regionId].Sick += 1;
        }

        public void SetRegions(List<Region> regions) => _simulation.SetRegions(regions);

        public void SetRegionsQueues() => _simulation.SetRegionQueues();

        public void SetDisease(string name, double speed, double deathProbability, int length, int immunityLength)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název nemoci nesmí být prázdný.");

            if (speed <= 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Rychlost šíření musí být kladné číslo.");

            if (deathProbability < 0 || deathProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(deathProbability), "Pravděpodobnost smrti musí být v rozmezí 0 až 1 (včetně).");

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Délka trvání nemoci musí být alespoň 1 den.");

            if (immunityLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Délka trvání imunity musí být alespoň 1 den.");


            _simulation.SetDisease(new Disease(name, speed, deathProbability, length, immunityLength));
            System.Diagnostics.Debug.WriteLine($"[SetDisease] Disease hash: {_simulation.Disease.GetHashCode()}, Abilities: {_simulation.Disease.Abilities.Count}");
            SetRegionsQueues();
            UpdateRegionsDiseaseValues();
        }

        public void SetSimulation()
        {
            _simulation = new Simulation();
            _simulation.OnDaySimulated += (update) => OnDaySimulated?.Invoke(update);
            _simulation.Logs.OnLogAdded += (log) => OnLogAdded?.Invoke(log);
        }


        // CONTROLLING METHODS
        public void ProcessPendingCommands() => _simulation.ProcessPendingCommands();

        public void ChangeSimulationSpeed(int speed)
        {
            if (speed >= 0)
                _simulation.EnqueueCommand(new ChangeSimulationSpeedCommand(speed));
            else
                throw new Exception("Hodnota musí být kladná nebo 0");
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

            _simulation.EnqueueCommand(new ChangeVaccineParametersCommand(protectionEfficiency, deathProtectionEfficiency));
        }

        public void StartVaccinatingAllRegions()
        {
            if (_simulation.Vaccine is null)
            {
                throw new Exception("Neexistuje vakcína, nejprve ji vytvořte");
            }

            _simulation.EnqueueCommand(new StartVaccinationAllRegionCommand());
        }

        public void StopVaccinatingAllRegions()
        {
            _simulation.EnqueueCommand(new StopVaccinationAllRegionCommand());
        }

        public void StartVaccinatingSingleRegion(int regionId)
            => _simulation.EnqueueCommand(new StartVaccinationSingleRegionCommand(GetRegion(regionId)));

        public void StopVaccinatingSingleRegion(int regionId)
            => _simulation.EnqueueCommand(new StopVaccinationSingleRegionCommand(GetRegion(regionId)));

        public void AddRegionAbility(int regionId, RegionAbility ability) => _simulation.EnqueueCommand(new AddRegionAbilityCommand(GetRegion(regionId), ability));

        public void RemoveRegionAbility(int regionId, RegionAbility ability) => _simulation.EnqueueCommand(new RemoveRegionAbilityCommand(GetRegion(regionId), ability));

        public void ChangeDefaultSpreadingSpeed(string? input)
        {
            if (input == null || !double.TryParse(input, out double defaultSpreadingSpeed))
                throw new ArgumentException("Zadejte číselnou hodnotu.");
            _simulation.EnqueueCommand(new ChangeDefaultSpreadingSpeedCommand(defaultSpreadingSpeed));
        }

        public void ChangeDeathProbability(string? input)
        {
            if (input == null || !double.TryParse(input, out double deathProbability) || !(deathProbability >= 0 && deathProbability <= 1))
                throw new ArgumentException("Zadejte číselnou hodnotu.");
            _simulation.EnqueueCommand(new ChangeDeathProbabilityCommand(deathProbability));
        }

        public void AddDiseaseAbility(DiseaseAbility ability) => _simulation.EnqueueCommand(new AddDiseaseAbilityCommand(ability));

        public void RemoveDiseaseAbility(DiseaseAbility ability) => _simulation.EnqueueCommand(new RemoveDiseaseAbilityCommand(ability));

        public void ChangeRegionSpreadingSpeed(int regionId, string value)
        {
            Region region = GetRegion(regionId);
            if (!double.TryParse(value, out double newSpreadingSpeed) || newSpreadingSpeed < 0)
                throw new ArgumentException("Neplatná hodnota.");
            _simulation.EnqueueCommand(new ChangeRegionSpreadingSpeedCommand(region, newSpreadingSpeed));
        }

        public void ChangeRegionHealthcareIndex(int regionId, string value)
        {
            Region region = GetRegion(regionId);
            if (!double.TryParse(value, out double newHealthcareIndex) || newHealthcareIndex < 0)
                throw new ArgumentException("Neplatná hodnota.");
            _simulation.EnqueueCommand(new ChangeRegionHealthcareIndexCommand(region, newHealthcareIndex));
        }

        public void StartSimulation()
        {
            if (_cts != null) return;
            _simulation.Run();
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => _simulation.Simulate(_cts.Token));
        }

        public void StopSimulation()
        {
            _simulation.Stop();
            _cts?.Cancel();
            _cts = null;
        }
    }
}