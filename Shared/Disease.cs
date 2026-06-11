using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
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
        public int ImmunityLength { get; set; }

        /// <summary>
        /// List of abilities applied to the disease.
        /// </summary>
        public List<DiseaseAbility> Abilities { get; set; }

        /// <summary>
        /// Probability of death for infected individuals.
        /// </summary>
        public double DefaultDeathProbability { get; set; }
        public double TotalDeathProbability { get; set; }

        /// <summary>
        /// Initializes a new disease with provided parameters.
        /// </summary>
        public Disease(string name, double defaultSpreadingSpeed, double deathProbability, int sicknessLength, int immunityLength ) { 
            Name = name;
            DefaultSpreadingSpeed = defaultSpreadingSpeed;
            TotalSpreadingSpeed = DefaultSpreadingSpeed;
            SicknessLength = sicknessLength;
            DefaultDeathProbability = deathProbability;
            TotalDeathProbability = DefaultDeathProbability;
            ImmunityLength = immunityLength;
            Abilities = new List<DiseaseAbility>();
        }

        /// <summary>
        /// Adds an ability to the disease and updates totals.
        /// </summary>
        public void AddAbility(DiseaseAbility ability)
        {
            Abilities.Add(ability);
            UpdateTotals();
        }

        /// <summary>
        /// Removes an ability if present and updates totals.
        /// </summary>
        public void RemoveAbility(DiseaseAbility ability)
        {
            if (Abilities.Contains(ability))
            {
                Abilities.Remove(ability);
                UpdateTotals();
            }
        }

        /// <summary>
        /// Changes the default spreading speed and recalculates totals.
        /// </summary>
        public void ChangeDefaultSpreadingSpeed (double newSpreadingSpeed)
        {
            DefaultSpreadingSpeed = newSpreadingSpeed;
            UpdateTotals();
        }
        
        /// <summary>
        /// Changes the death probability.
        /// </summary>
        public void ChangeDeathProbability(double newDeathProbability)
        {
            DefaultDeathProbability = newDeathProbability;
            UpdateTotals();
        }

        /// <summary>
        /// Recomputes TotalSpreadingSpeed by applying ability modifiers.
        /// </summary>
        private void UpdateTotals()
        {
            double tss = DefaultSpreadingSpeed;
            double dp = DefaultDeathProbability;

            foreach(DiseaseAbility ability in Abilities)
            {
                tss *= ability.SpreadingModifier;
                dp *= ability.DeathModifier;
            }

            TotalSpreadingSpeed = tss;
            TotalDeathProbability = dp;
        }
    }
}
