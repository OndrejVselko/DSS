using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Represents a disease with base parameters and applied abilities.
    /// </summary>
    public class Disease
    {
        /// <summary>
        /// Disease name.
        /// </summary>
        public string Name {  get; set; } = string.Empty;

        /// <summary>
        /// Base spreading speed before abilities.
        /// </summary>
        public double DefaultSpreadingSpeed { get; set; }

        /// <summary>
        /// Effective spreading speed after applying abilities.
        /// </summary>
        public double TotalSpreadingSpeed { get; set; }

        /// <summary>
        /// Length of sickness in days.
        /// </summary>
        public int SicknessLength { get; set; }

        /// <summary>
        /// List of abilities applied to the disease.
        /// </summary>
        public List<DiseaseAbility> Abilities { get; set; }

        /// <summary>
        /// Probability of death for infected individuals.
        /// </summary>
        public double DeathProbability { get; set; }

        /// <summary>
        /// Initializes a new disease with provided parameters.
        /// </summary>
        public Disease(string name, double defaultSpreadingSpeed, double deathProbability, int sicknessLength ) { 
            Name = name;
            DefaultSpreadingSpeed = defaultSpreadingSpeed;
            SicknessLength = sicknessLength;
            DeathProbability = deathProbability;
            Abilities = new List<DiseaseAbility>();
        }

        /// <summary>
        /// Adds an ability to the disease and updates totals.
        /// </summary>
        public void AddAbility(DiseaseAbility ability)
        {
            Abilities.Add(ability);
            UpdateTotalSpreadingSpeed();
        }

        /// <summary>
        /// Removes an ability if present and updates totals.
        /// </summary>
        public void RemoveAbility(DiseaseAbility ability)
        {
            if (Abilities.Contains(ability))
            {
                Abilities.Remove(ability);
                UpdateTotalSpreadingSpeed();
            }
        }

        /// <summary>
        /// Changes the default spreading speed and recalculates totals.
        /// </summary>
        public void ChangeDefaultSpreadingSpeed (double newSpreadingSpeed)
        {
            DefaultSpreadingSpeed = newSpreadingSpeed;
            UpdateTotalSpreadingSpeed();
        }
        
        /// <summary>
        /// Changes the death probability.
        /// </summary>
        public void ChangeDeathProbability(double newDeathProbability)
        {
            DeathProbability = newDeathProbability;
        }

        /// <summary>
        /// Recomputes TotalSpreadingSpeed by applying ability modifiers.
        /// </summary>
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
