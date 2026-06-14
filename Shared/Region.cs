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
        public int Population { get; set; }
        public int Sick { get; set; } = 0;
        public int Dead { get; set; } = 0;
        public int Vaccinated { get; set; } = 0;
        public int Immune { get; set; }


        public bool Vaccinating { get; set; }
        public Vaccine vaccine { get; set; }
        public double TotalVaccinatingCapacity { get; set; }
        public double HealthcareIndex { get; set; }
        public double TotalSpreadingSpeed { get; set; }
        public double TotalDeathProbability { get; set; }
        public double RegionSpreadingSpeed { get; set; }
        public double RegionDeathPropability { get; set; } = 1;
        public double DiseaseSpreadingSpeed { get; set; }
        public double DiseaseDeathPropability { get; set; }
        public Queue<int> ImmunityHistory;
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

        public StatisticUpdate LastUpdate { get; set; } = new StatisticUpdate(0, 0, 0, 0, 0, 0)

        public Region()
        {
            Abilities = new List<RegionAbility>();
            ActiveInteractions = new List<Interaction>();
            ImmunityHistory = new Queue<int>();
            TotalVaccinatingCapacity = 0.001;
            InteractionSpreadingModifier = 1.0;
            InteractionDeathModifier = 1.0;
        }

        /// <summary>
        /// Inicializuje frontu imunity nulami.
        /// </summary>
        public void SetStartingQueue(int sicknessLength, int immunityLength)
        {
            SicknessLength = sicknessLength;
            ImmunityLength = immunityLength;
            ImmunityHistory = new Queue<int>();
            for (int i = 0; i < immunityLength; i++)
                ImmunityHistory.Enqueue(0);
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

        public void RecalculateRandomOccurrence(int globalSick, long globalPopulation)
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

        public StatisticUpdate SimulateDay()
        {
            double localDeathProtectionEfficiency = 0;
            double localProtectionEfficiency = 0;
            if (vaccine is not null)
            {
                localDeathProtectionEfficiency = vaccine.DeathProtectionEfficiency;
                localProtectionEfficiency = vaccine.ProtectionEfficiency;
            }

            // --- Uzdravování ---
            double recoveryChance = 1.0 / Math.Max(SicknessLength, 1);
            int recovering = ProbabilisticFloor(Sick * recoveryChance);
            recovering = Math.Min(recovering, Sick);

            // --- Úmrtí ---
            int deathGrowth = ProbabilisticFloor(recovering * TotalDeathProbability);
            deathGrowth -= ProbabilisticFloor(((double)Vaccinated / Math.Max(Population, 1)) * deathGrowth * localDeathProtectionEfficiency);
            deathGrowth = Math.Clamp(deathGrowth, 0, recovering);

            int newlyRecovered = recovering - deathGrowth;

            // --- Imunita ---
            int losingImmunity = ImmunityHistory.Dequeue();
            ImmunityHistory.Enqueue(newlyRecovered);
            Immune += newlyRecovered - losingImmunity;
            Immune = Math.Max(Immune, 0);

            // --- Nové nákazy ---
            int susceptible = Math.Max(Population - Sick - Dead - Immune - Vaccinated, 0);

            int newInfections = ProbabilisticFloor(Sick * TotalSpreadingSpeed * ((double)susceptible / Math.Max(Population, 1)));
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
            int newVaccinated = 0;
            if (Vaccinating)
            {
                double capacity = TotalVaccinatingCapacity > 0
                    ? TotalVaccinatingCapacity
                    : Population * 0.001;
                int remaining = Math.Max(Population - Dead - Vaccinated - Immune - Sick, 0);
                newVaccinated = Math.Min(ProbabilisticFloor(Population * capacity), remaining);
                Vaccinated = Math.Min(Vaccinated + newVaccinated, Population - Dead);
            }

            LastUpdate = new StatisticUpdate(newInfections, deathGrowth, newVaccinated, Sick, Dead, Vaccinated);
            return LastUpdate;
        }

        public void UpdateDiseaseValues(double diseaseSpreadingSpeed, double diseaseDeathProbability)
        {
            DiseaseSpreadingSpeed = diseaseSpreadingSpeed;
            DiseaseDeathPropability = diseaseDeathProbability;
        }

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

        public void StartVaccinating() => Vaccinating = true;
        public void StopVaccinating() => Vaccinating = false;
        public void SetVaccine(Vaccine vaccine) => this.vaccine = vaccine;

        public override string ToString() => Name;

        public string ToStringFull()
        {
            string result = $"[{Id}] {Name}, populace: {Population}, index zdravotnictví: {HealthcareIndex} \n" +
                $"nemocní: {Sick}, mrtví: {Dead}, očkovaní: {Vaccinated}, imunní: {Immune}\n" +
                $"zákl. rychlost šíření: {RegionSpreadingSpeed}, rychlost šíření celkem: {TotalSpreadingSpeed}\n" +
                $"zákl. šance na úmrtí: {RegionDeathPropability}, šance na úmrtí celkem: {TotalDeathProbability}\n" +
                $"Vlastnosti regionu: ";
            if (Abilities.Count > 0)
                foreach (RegionAbility ability in Abilities)
                    result += ability.ToString() + "\n";
            else
                result += "\\";
            return result;
        }

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

        public string InteractionsToString()
        {
            string result = string.Empty;
            foreach (Interaction interaction in ActiveInteractions)
                result += interaction.ToString() + "\n";
            return result != string.Empty ? result : "Žádné interakce";
        }

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