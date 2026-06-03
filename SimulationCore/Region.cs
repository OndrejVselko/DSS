using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Represents a region in the simulation with demographics and abilities.
    /// </summary>
    public class Region
    {
        private readonly Random _random = new Random();
        /// <summary>Region identifier.</summary>
        public int Id { get; set; }
        /// <summary>Region name.</summary>
        public string Name { get; set; }
        /// <summary>Population count.</summary>
        public int Population { get; set; }
        /// <summary>Currently sick individuals.</summary>
        public int Sick { get; set; }
        /// <summary>Accumulated dead individuals.</summary>
        public int Dead { get; set; }
        /// <summary>Accumulated vaccinated individuals.</summary>
        public int Vaccinated { get; set; }

        public bool Vaccinating { get; set; }

        public Vaccine vaccine { get; set; }
        public double TotalVaccinatingCapacity { get; set; }
        /// <summary>Healthcare quality index for the region.</summary>
        public double HealthcareIndex { get; set; }
        /// <summary>Effective total spreading speed for the region.</summary>
        public double TotalSpreadingSpeed { get; set; }
        /// <summary>Effective total death probability for the region.</summary>
        public double TotalDeathProbability { get; set; }
        /// <summary>Base region spreading speed.</summary>
        public double RegionSpreadingSpeed { get; set; }
        /// <summary>Region-specific death multiplier.</summary>
        public double RegionDeathPropability { get; set; } = 1;
        /// <summary>Spreading speed contributed by disease.</summary>
        public double DiseaseSpreadingSpeed { get; set; }
        /// <summary>Death probability contributed by disease.</summary>
        public double DiseaseDeathPropability { get; set; }
        /// <summary>Abilities applied to the region.</summary>
        public List<RegionAbility> Abilities { get; set; }
        /// <summary>Queue tracking recent sick counts (by day).</summary>
        public Queue<int> SickHistory;

        public List<Region> NeighbouringRegions { get; set; } = new List<Region>();

        public List<int> NeighbourIds { get; set; } = new();

        public double TotalRandomOccurrence { get; set; } = 0;

        public double BorderAbilityModifier { get; private set; } = 1.0;

        public List<Interaction> ActiveInteractions;

        public double InteractionSpreadingModifier;

        public double InteractionDeathModifier;





        /// <summary>
        /// Initializes region collections.
        /// </summary>
        public Region()
        {
            SickHistory = new Queue<int>();
            Abilities = new List<RegionAbility>();
            ActiveInteractions = new List<Interaction>(); 
            TotalVaccinatingCapacity = 0.001 * Population;
            InteractionSpreadingModifier = 1.0; 
            InteractionDeathModifier = 1.0; 
        }

        public void SetNeighbouringRegions(List<Region> regions)
        {
            NeighbouringRegions = regions;
        }


        /// <summary>
        /// Pre-fills sick history queue with zeros for the given days.
        /// </summary>
        public void SetStartingQueue(int days)
        {
            for (int i = 0; i < days; i++)
            {
                SickHistory.Enqueue(0);
            }
        }

        /// <summary>
        /// Adds a region ability and updates region values.
        /// </summary>
        public void AddAbility(RegionAbility ability)
        {
            Abilities.Add(ability);
            UpdateRegionValues();
        }

        /// <summary>
        /// Removes a region ability and updates region values.
        /// </summary>
        public void RemoveAbility(RegionAbility ability)
        {
            Abilities.Remove(ability);
            UpdateRegionValues();
        }

        /// <summary>
        /// Recomputes region-level totals using region and disease modifiers.
        /// </summary>

        public void RecalculateRandomOccurrence(int globalSick, int globalPopulation)
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
            double deathProbability = HealthcareIndex * DiseaseDeathPropability;
            double spreadingSpeed = RegionSpreadingSpeed * DiseaseSpreadingSpeed;
            double borderModifier = 1.0;
            double vaccinationCapacity = Population * 0.001;

            foreach (RegionAbility ability in Abilities)
            {
                spreadingSpeed *= ability.SpreadingModifier;
                deathProbability *= ability.DeathModifier;
                borderModifier *= ability.BorderModifier;
                vaccinationCapacity *= ability.VaccinationCapacityModifier;
            }
            spreadingSpeed *= InteractionSpreadingModifier; //  Tady bude místo jedničky funkce Interakce, která vrácí hodnotu pro region
            deathProbability *= InteractionDeathModifier; //  Tady bude místo jedničky funkce Interakce, která vrácí hodnotu pro region
            TotalSpreadingSpeed = spreadingSpeed;
            TotalDeathProbability = deathProbability;
            BorderAbilityModifier = borderModifier;
            TotalVaccinatingCapacity = vaccinationCapacity;

        }

        /// <summary>
        /// Simulates a single day for the region and returns statistics delta.
        /// </summary>
        public StatisticUpdate SimulateDay()
        {
            double localDeathProtectionEfficiency = 0;
            double localProtectionEfficiency = 0;
            if (vaccine is not null)
            {
                localDeathProtectionEfficiency = vaccine.DeathProtectionEfficiency;
                localProtectionEfficiency = vaccine.ProtectionEfficiency;
            }
            int finishingSick = this.SickHistory.Dequeue();

            int deathGrowth = (int)Math.Floor(finishingSick * TotalDeathProbability);
            deathGrowth -= (int)Math.Floor(((double)Vaccinated / Population) * deathGrowth * localDeathProtectionEfficiency);
            deathGrowth = Math.Min(deathGrowth, finishingSick);

            int newInfections = (int)Math.Floor(this.Sick * TotalSpreadingSpeed);
            newInfections -= (int)Math.Floor(((double)Vaccinated / Population) * newInfections * localProtectionEfficiency);
            int susceptible = this.Population - this.Sick - this.Dead;
            newInfections = Math.Min(newInfections, susceptible);

            SickHistory.Enqueue(newInfections);

            Sick += newInfections - finishingSick;

            Dead += deathGrowth;

            int newVaccinated = 0;
            if (Vaccinating && Vaccinated < Population - Dead)
            {
                newVaccinated = Math.Min((int)(Math.Floor(Population * TotalVaccinatingCapacity)), Population - Dead - Vaccinated);
                Vaccinated += newVaccinated;
            }

            if (_random.NextDouble() < TotalRandomOccurrence && Population - Sick - Dead - Vaccinated > 0)
            {
                Sick += 1;
                newInfections += 1;
            }


            if (this.Sick < 0)
                this.Sick = 0;

            return new StatisticUpdate(newInfections, deathGrowth, newVaccinated, Sick, Dead, Vaccinated);
        }

        /// <summary>
        /// Updates disease-related values used by the region.
        /// </summary>
        public void UpdateDiseaseValues(double diseaseSpreadingSpeed, double diseaseDeathProbability)
        {
            DiseaseSpreadingSpeed = diseaseSpreadingSpeed;
            DiseaseDeathPropability = diseaseDeathProbability;
        }

        /// <summary>
        /// Changes base spreading speed for the region and recalculates totals.
        /// </summary>
        public void ChangeSpreadingSpeed(double newSpeed)
        {
            RegionSpreadingSpeed = newSpeed;
            UpdateRegionValues();
        }

        /// <summary>
        /// Changes healthcare index for the region and recalculates totals.
        /// </summary>
        public void ChangeHealtcareIndex(double healtcareIndex)
        {
            this.HealthcareIndex = healtcareIndex;
            UpdateRegionValues();
        }

        /// <summary>
        /// Returns a multi-line string describing region state and abilities.
        /// </summary>

        public void StartVaccinating()
        {
            Vaccinating = true;
        }

        public void StopVaccinating()
        {
            Vaccinating = false;
        }

        public void SetVaccine(Vaccine vaccine)
        {
            this.vaccine = vaccine;
        }

        public override string ToString()
        {
            string result = $"[{Id}] {Name}, populace: {Population}, index zdravotnictví: {HealthcareIndex} \n" +
                $"nemocní: {Sick}, mrtví: {Dead}, očkovaní: {Vaccinated}\n" +
                $"zákl. rychlost šíření: {RegionSpreadingSpeed}, rychlost šíření celkem: {TotalSpreadingSpeed}\n" +
                $"zákl. šance na úmrtí: {RegionDeathPropability}, šance na úmrtí celkem: {TotalSpreadingSpeed}\n" +
                $"Vlastnosti regionu: ";
            if (Abilities.Count > 0)
            {
                foreach (RegionAbility ability in Abilities)
                {
                    result += ability.ToString() + "\n";
                }
            }
            else
            {
                result += "\\";
            }

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

        public String InteractionsToString()
        {
            string result = string.Empty;
            foreach (Interaction interaction in ActiveInteractions)
            {
                result += interaction.ToString() + "\n";
            }

            if (result != string.Empty)
                return result;
            else return "Žádné interakce";
        }
    }
}
