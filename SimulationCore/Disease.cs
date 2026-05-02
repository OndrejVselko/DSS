using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Disease
    {
        public string Name {  get; set; } = string.Empty;
        public double DefaultSpreadingSpeed { get; set; }
        public double TotalSpreadingSpeed { get; set; }
        public int SicknessLength { get; set; }
        public List<DiseaseAbility> Abilities { get; set; }
        public double DeathProbability { get; set; }
        public Disease(string name, double defaultSpreadingSpeed, double deathProbability, int sicknessLength ) { 
            Name = name;
            DefaultSpreadingSpeed = defaultSpreadingSpeed;
            SicknessLength = sicknessLength;
            DeathProbability = deathProbability;
            Abilities = new List<DiseaseAbility>();
        }

        public void AddAbility(DiseaseAbility ability)
        {
            Abilities.Add(ability);
            UpdateTotalSpreadingSpeed();
        }

        public void RemoveAbility(DiseaseAbility ability)
        {
            if (Abilities.Contains(ability))
            {
                Abilities.Remove(ability);
                UpdateTotalSpreadingSpeed();
            }
        }

        public void ChangeDefaultSpreadingSpeed (double newSpreadingSpeed)
        {
            DefaultSpreadingSpeed = newSpreadingSpeed;
            UpdateTotalSpreadingSpeed();
        }
        
        public void ChangeDeathProbability(double newDeathProbability)
        {
            DeathProbability = newDeathProbability;
        }

        private void UpdateTotalSpreadingSpeed()
        {
            double tss = DefaultSpreadingSpeed;
            foreach(DiseaseAbility ability in Abilities)
            {
                tss *= ability.PrimaryModifier;
            }

            TotalSpreadingSpeed = tss;
        }
    }
}
