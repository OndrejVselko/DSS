using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class Region
    {
        private readonly Random _random = new Random();

        public int Id { get; set; }
        public string Name { get; set; }
        public string IsoCode { get; set; } = string.Empty;
        public long Population { get; set; }
        public long Sick { get; set; } = 0;
        public long Dead { get; set; } = 0;
        public long Vaccinated { get; set; } = 0;
        public long Immune { get; set; }


        public bool Vaccinating { get; set; }
        public Vaccine Vaccine { get; set; }
        public double TotalVaccinatingCapacity { get; set; }
        public double HealthcareIndex { get; set; }
        public double TotalSpreadingSpeed { get; set; }
        public double TotalDeathProbability { get; set; }
        public double RegionSpreadingSpeed { get; set; }
        public double RegionDeathPropability { get; set; } = 1;
        public double DiseaseSpreadingSpeed { get; set; }
        public double DiseaseDeathPropability { get; set; }
        public Queue<long> ImmunityHistory;
        public int ImmunityLength { get; set; }

        public List<RegionAbility> Abilities { get; set; }
        public List<int> AbilityIds { get; set; } = new();
        public List<Region> NeighbouringRegions { get; set; } = new();
        public List<int> NeighbourIds { get; set; } = new();
        public double TotalRandomOccurrence { get; set; } = 0;
        public double BorderAbilityModifier { get; private set; } = 1.0;
        public List<Interaction> ActiveInteractions;
        public double InteractionSpreadingModifier;
        public double InteractionDeathModifier;
        public int SicknessLength { get; set; }

        public StatisticUpdate LastUpdate { get; set; } = new StatisticUpdate(0, 0, 0, 0, 0, 0);

        public Region()
        {
            Abilities = new List<RegionAbility>();
            ActiveInteractions = new List<Interaction>();
            ImmunityHistory = new Queue<long>();
            TotalVaccinatingCapacity = 0.001;
            InteractionSpreadingModifier = 1.0;
            InteractionDeathModifier = 1.0;
        }


        // SETTING METHODS
        public void SetStartingQueue(int sicknessLength, int immunityLength)
        {
            SicknessLength = sicknessLength;
            ImmunityLength = immunityLength;
            ImmunityHistory = new Queue<long>();
            for (int i = 0; i < immunityLength; i++)
                ImmunityHistory.Enqueue(0);
        }

        public void SetVaccine(Vaccine vaccine) => this.Vaccine = vaccine;


        // UPDATING METHODS

        public void RecalculateRandomOccurrence(long globalSick, long globalPopulation)
        {
            double globalRatio = (double)globalSick / globalPopulation;
            double neighbourSick = NeighbouringRegions.Sum(r => r.Sick);
            double neighbourPopulation = NeighbouringRegions.Sum(r => r.Population);
            double neighbourRatio = neighbourPopulation > 0 ? neighbourSick / neighbourPopulation : 0;

            double raw = (globalRatio * 0.05) + (neighbourRatio * 0.95);
            TotalRandomOccurrence = Math.Max(Math.Min(raw * BorderAbilityModifier, 0.8), 0);
        }

        public void UpdateRegionValues()
        {
            double deathProbability = DiseaseDeathPropability / Math.Max(HealthcareIndex, 0.01);
            double spreadingSpeed = RegionSpreadingSpeed * DiseaseSpreadingSpeed;
            double borderModifier = 1.0;
            double vaccinationCapacity = 0.001;

            foreach (RegionAbility ability in Abilities)
            {
                spreadingSpeed *= ability.SpreadingModifier;
                deathProbability *= ability.DeathModifier;
                borderModifier *= ability.BorderModifier;
                vaccinationCapacity *= ability.VaccinationCapacityModifier;
            }

            spreadingSpeed *= InteractionSpreadingModifier;
            deathProbability *= InteractionDeathModifier;
            TotalSpreadingSpeed = spreadingSpeed;
            TotalDeathProbability = deathProbability;
            BorderAbilityModifier = borderModifier;
            TotalVaccinatingCapacity = vaccinationCapacity;
        }

        public void UpdateDiseaseValues(double diseaseSpreadingSpeed, double diseaseDeathProbability)
        {
            DiseaseSpreadingSpeed = diseaseSpreadingSpeed;
            DiseaseDeathPropability = diseaseDeathProbability;
        }

        private void RecalculateInteractionModifiers()
        {
            double totalSpreadingInteraction = 1;
            double totalDeathInteraction = 1;
            foreach (Interaction interaction in ActiveInteractions)
            {
                totalDeathInteraction *= interaction.DeathModifier;
                totalSpreadingInteraction *= interaction.SpreadingModifier;
            }
            InteractionSpreadingModifier = totalSpreadingInteraction;
            InteractionDeathModifier = totalDeathInteraction;
            UpdateRegionValues();
        }


        // SIMULATION METHODS
        public StatisticUpdate SimulateDay()
        {
            double localDeathProtectionEfficiency = 0;
            double localProtectionEfficiency = 0;
            if (Vaccine is not null)
            {
                localDeathProtectionEfficiency = Vaccine.DeathProtectionEfficiency;
                localProtectionEfficiency = Vaccine.ProtectionEfficiency;
            }

            // --- Uzdravování ---
            double recoveryChance = 1.0 / Math.Max(SicknessLength, 1);
            long recovering = ProbabilisticFloor(Sick * recoveryChance);
            recovering = Math.Min(recovering, Sick);

            // --- Úmrtí ---
            long deathGrowth = ProbabilisticFloor(recovering * TotalDeathProbability);
            deathGrowth -= ProbabilisticFloor(((double)Vaccinated / Math.Max(Population, 1)) * deathGrowth * localDeathProtectionEfficiency);
            deathGrowth = Math.Clamp(deathGrowth, 0, recovering);

            long newlyRecovered = recovering - deathGrowth;

            // --- Imunita ---
            long losingImmunity = ImmunityHistory.Dequeue();
            ImmunityHistory.Enqueue(newlyRecovered);
            Immune += newlyRecovered - losingImmunity;
            Immune = Math.Max(Immune, 0);

            // --- Nové nákazy ---
            long susceptible = Math.Max(Population - Sick - Dead - Immune - Vaccinated, 0);

            long newInfections = ProbabilisticFloor(Sick * TotalSpreadingSpeed * ((double)susceptible / Math.Max(Population, 1)));
            newInfections -= ProbabilisticFloor(((double)Vaccinated / Math.Max(Population, 1)) * newInfections * localProtectionEfficiency);
            newInfections = Math.Clamp(newInfections, 0, susceptible);

            // --- Aktualizace stavu ---
            Sick += newInfections - recovering;
            Sick = Math.Max(Sick, 0);
            Dead += deathGrowth;
            Dead = Math.Min(Dead, Population);

            // --- Náhodný výskyt ---
            if (_random.NextDouble() < TotalRandomOccurrence && susceptible > 0)
            {
                Sick += 1;
                newInfections += 1;
            }

            // --- Očkování ---
            long newVaccinated = 0;
            if (Vaccinating)
            {
                double capacity = TotalVaccinatingCapacity > 0
                    ? TotalVaccinatingCapacity
                    : Population * 0.001;
                long remaining = Math.Max(Population - Dead - Vaccinated - Immune - Sick, 0);
                newVaccinated = Math.Min(ProbabilisticFloor(Population * capacity), remaining);
                Vaccinated = Math.Min(Vaccinated + newVaccinated, Population - Dead);
            }

            LastUpdate = new StatisticUpdate(newInfections, deathGrowth, newVaccinated, Sick, Dead, Vaccinated);
            return LastUpdate;
        }

        // CONTROLLING METHODS

        public void ChangeSpreadingSpeed(double newSpeed)
        {
            RegionSpreadingSpeed = newSpeed;
            UpdateRegionValues();
        }

        public void ChangeHealtcareIndex(double healtcareIndex)
        {
            HealthcareIndex = healtcareIndex;
            UpdateRegionValues();
        }

        public void AddAbility(RegionAbility ability)
        {
            Abilities.Add(ability);
            UpdateRegionValues();
        }

        public void RemoveAbility(RegionAbility ability)
        {
            Abilities.Remove(ability);
            UpdateRegionValues();
        }

        public void StartVaccinating() => Vaccinating = true;
        public void StopVaccinating() => Vaccinating = false;

        public void AddInteraction(Interaction interaction)
        {
            ActiveInteractions.Add(interaction);
            RecalculateInteractionModifiers();
        }

        public void RemoveInteraction(Interaction interaction)
        {
            if (ActiveInteractions.Contains(interaction))
            {
                ActiveInteractions.Remove(interaction);
                RecalculateInteractionModifiers();
            }
        }

        // OTHER METHODS
        public override string ToString() => Name;

        private int ProbabilisticFloor(double value)
        {
            if (value <= 0) return 0;
            if (value >= int.MaxValue) return int.MaxValue;
            int floor = (int)Math.Floor(value);
            double remainder = value - floor;
            return floor + (_random.NextDouble() < remainder ? 1 : 0);
        }
    }
}