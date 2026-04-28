using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Disease
    {
        public string name {  get; set; } = string.Empty;
        public double defaultSpreadingSpeed { get; set; }
        public double totalSpreadingSpeed { get; set; }
        public int sicknessLength { get; set; }
        public List<DiseaseAbility> abilities { get; set; }

        public Disease(string name, double defaultSpreadingSpeed, int sicknessLength) { 
            this.name = name;
            this.defaultSpreadingSpeed = defaultSpreadingSpeed;
            this.sicknessLength = sicknessLength;
            abilities = new List<DiseaseAbility>();
        }

        public void addAbility(DiseaseAbility ability)
        {
            abilities.Add(ability);
            updateTotalSpreadingSpeed();
        }

        public void removeAbility(DiseaseAbility ability)
        {
            abilities.Remove(ability);
            updateTotalSpreadingSpeed();
        }

        public void changeDefaultSpreadingSpeed (double newSpreadingSpeed)
        {
            this.defaultSpreadingSpeed = newSpreadingSpeed;
        }

        private void updateTotalSpreadingSpeed()
        {
            // Dodelat az bude disease a region ability
        }
    }
}
