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
        public List<RegionAbility> abilities { get; set; }

        public Region()
        {
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
            StatisticUpdate update = new StatisticUpdate(0,0,0);
            // Tady se bude odehravat kompletni vypocet deni zmeny

            return update;
        }
    }
}
