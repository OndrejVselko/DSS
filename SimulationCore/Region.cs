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
        /// <summary>Region identifier.</summary>
        public int Id { get; set; }
        /// <summary>Region name.</summary>
        public string Name { get; set; }
        /// <summary>Population count.</summary>
        public int Population { get; set; }
        /// <summary>Currently sick individuals.</summary>
        public int Sick {  get; set; }
        /// <summary>Accumulated dead individuals.</summary>
        public int Dead {  get; set; }
        /// <summary>Accumulated vaccinated individuals.</summary>
        public int Vaccinated {  get; set; }
        /// <summary>Healthcare quality index for the region.</summary>
        public double HealthcareIndex { get; set; }
        /// <summary>Effective total spreading speed for the region.</summary>
        public double TotalSpreadingSpeed { get; set; }
        /// <summary>Effective total death probability for the region.</summary>
        public double TotalDeathProbability { get; set; }
        /// <summary>Base region spreading speed.</summary>
        public double RegionSpreadingSpeed {  get; set; }
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

        /// <summary>
        /// Initializes region collections.
        /// </summary>
        public Region()
        {
            SickHistory = new Queue<int>();       
            Abilities = new List<RegionAbility>();
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
        public void UpdateRegionValues()
        {
            double spreadingSpeed = RegionSpreadingSpeed * DiseaseSpreadingSpeed;
            foreach (RegionAbility ability in Abilities) {
                spreadingSpeed *= ability.PrimaryModifier;
            }

            spreadingSpeed *= 1;//  Tady bude místo jedničky funkce Interakce, která vrácí hodnotu pro region
            TotalSpreadingSpeed = spreadingSpeed;
            TotalDeathProbability = DiseaseDeathPropability;
        }

        /// <summary>
        /// Simulates a single day for the region and returns statistics delta.
        /// </summary>
        public StatisticUpdate SimulateDay()
        {
            int finishingSick = this.SickHistory.Dequeue();

            int deathGrowth = (int)Math.Floor(finishingSick * TotalDeathProbability);
            deathGrowth = Math.Min(deathGrowth, finishingSick);

            int newInfections = (int)Math.Floor(this.Sick * TotalSpreadingSpeed);
            int susceptible = this.Population - this.Sick - this.Dead;
            newInfections = Math.Min(newInfections, susceptible);

            SickHistory.Enqueue(newInfections);

            Sick += newInfections - finishingSick;

            Dead += deathGrowth;

            if (this.Sick < 0) 
                this.Sick = 0;
            int newVaccinated = 0;
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
        public override string ToString()
        {
            string result = $"[{Id}] {Name}, populace: {Population}, index zdravotnictví: {HealthcareIndex} \n" +
                $"nemocní: {Sick}, mrtví: {Dead}, očkovaní: {Vaccinated}\n" +
                $"zákl. rychlost šíření: {RegionSpreadingSpeed}, rychlost šíření celkem: {TotalSpreadingSpeed}\n" +
                $"zákl. šance na úmrtí: {RegionDeathPropability}, šance na úmrtí celkem: {TotalSpreadingSpeed}\n" +
                $"Vlastnosti regionu: ";
            if (Abilities.Count > 0) {
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
    }
}
