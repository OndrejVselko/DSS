using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class Disease
    {
        public string name {  get; set; } = string.Empty;
        public double spreadingSpeed { get; set; }
        public int sicknessLength { get; set; }
        public List<DiseaseAbility> abilities { get; set; }

        public Disease(string name, double spreadingSpeed, int sicknessLength) { 
            this.name = name;
            this.spreadingSpeed = spreadingSpeed;
            this.sicknessLength = sicknessLength;
            abilities = new List<DiseaseAbility>();
        }

        public void addAbility(DiseaseAbility ability)
        {
            abilities.Add(ability);
            updateSpreadingSpeed();
        }

        public void removeAbility(DiseaseAbility ability)
        {
            abilities.Remove(ability);
            updateSpreadingSpeed();
        }

        private void updateSpreadingSpeed()
        {
            // Dodelat az bude disease a region ability
        }
    }
}
