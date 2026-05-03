using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        public int Sick {  get; set; }
        public int Dead {  get; set; }
        public int Vaccinated {  get; set; }
        public double HealthcareIndex { get; set; }
        public double TotalSpreadingSpeed { get; set; }
        public double TotalDeathProbability { get; set; }
        public double RegionSpreadingSpeed {  get; set; }
        public double RegionDeathPropability { get; set; } = 1;
        public double DiseaseSpreadingSpeed { get; set; }
        public double DiseaseDeathPropability { get; set; }
        public List<RegionAbility> Abilities { get; set; }
        public Queue<int> SickHistory;

        public Region()
        {
            SickHistory = new Queue<int>();       
            Abilities = new List<RegionAbility>();
        }

        public void SetStartingQueue(int days)
        {
            for (int i = 0; i < days; i++)
            {
                SickHistory.Enqueue(0);
            }
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
            this.HealthcareIndex = healtcareIndex;
            UpdateRegionValues();
        }

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
