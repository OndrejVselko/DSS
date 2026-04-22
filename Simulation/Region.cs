using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
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
        public List<RegionAbility> abilities { get; set; }

        public Region(int id, string name, int population, int vaccinated, double healthcareIndex, double spreadingSpeed)
        {
            this.id = id;
            this.name = name;
            this.population = population;
            this.vaccinated = vaccinated;
            this.healthcareIndex = healthcareIndex;
            this.spreadingSpeed = spreadingSpeed;
            abilities = new List<RegionAbility>();
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

        public void updateSpreadingSpeed()
        {
            // Tohle opet dodelat, az budou ready ability
        }

        public void simulateDay()
        {
            // Tady se bude odehravat kompletni vypocet deni zmeny
        }
    }
}
