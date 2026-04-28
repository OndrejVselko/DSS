using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Region
    {
        public int id { get; set; }
        public string name { get; set; }
        public int population { get; set; }
        public int sick {  get; set; }
        public int dead {  get; set; }
        public int vaccinated {  get; set; }
        public double healthcareIndex { get; set; }
        public double spreadingSpeed {  get; set; }
        public double deathPropability { get; set; }
        public List<RegionAbility> abilities { get; set; }
        public Queue<int> sickHistory;

        public Region()
        {
            sickHistory = new Queue<int>();       
            abilities = new List<RegionAbility>();
        }

        public void setStartingQueue(int days)
        {
            for (int i = 0; i < days; i++)
            {
                sickHistory.Enqueue(0);
            }
        }

        public void addAbility(RegionAbility ability)
        {
            abilities.Add(ability);
            updateSpreadingSpeed();
        }

        public void removeAbility(RegionAbility ability) 
        { 
            abilities.Remove(ability);
            updateSpreadingSpeed();
        }

        public void changeHealtcareIndex (double healtcareIndex)
        {
            this.healthcareIndex = healtcareIndex;
        }

        public void updateSpreadingSpeed()
        {
            // Tohle opet dodelat, az budou ready ability
        }

        public StatisticUpdate simulateDay()
        {
            int finishingSick = this.sickHistory.Dequeue();

            int deathGrowth = (int)Math.Floor(finishingSick * deathPropability);
            deathGrowth = Math.Min(deathGrowth, finishingSick);

            int newInfections = (int)Math.Floor(this.sick * spreadingSpeed);
            int susceptible = this.population - this.sick - this.dead;
            newInfections = Math.Min(newInfections, susceptible);

            this.sickHistory.Enqueue(newInfections);

            this.sick += newInfections - finishingSick;

            this.dead += deathGrowth;

            if (this.sick < 0) 
                this.sick = 0;

            return new StatisticUpdate(newInfections, deathGrowth, 0);
        }
    }
}
