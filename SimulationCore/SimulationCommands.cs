using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    // Disease Commands

    /// <summary>
    /// Command that adds a disease ability to the current disease.
    /// </summary>
    public class AddDiseaseAbilityCommand : ISimulationCommand
    {
        private readonly DiseaseAbility _ability;

        public AddDiseaseAbilityCommand(DiseaseAbility ability)
        {
            _ability = ability;
        }

        /// <summary>Executes the command and updates regions.</summary>
        public void Execute(Simulation simulation)
        {
            simulation.disease.AddAbility(_ability);
            simulation.UpdateAllRegions();
        }
    }

    /// <summary>
    /// Command that removes a disease ability from the current disease.
    /// </summary>
    public class RemoveDiseaseAbilityCommand : ISimulationCommand
    {
        private readonly DiseaseAbility _ability;

        public RemoveDiseaseAbilityCommand(DiseaseAbility ability)
        {
            _ability = ability;
        }

        /// <summary>Executes the removal and updates regions.</summary>
        public void Execute(Simulation simulation)
        {
            simulation.disease.RemoveAbility(_ability);
            simulation.UpdateAllRegions();
        }
    }

    /// <summary>
    /// Command that changes the disease default spreading speed.
    /// </summary>
    public class ChangeDefaultSpreadingSpeedCommand : ISimulationCommand
    {
        private readonly double _newSpeed;

        public ChangeDefaultSpreadingSpeedCommand(double newSpeed)
        {
            _newSpeed = newSpeed;
        }

        /// <summary>Applies new speed and updates regions.</summary>
        public void Execute(Simulation simulation)
        {
            simulation.disease.ChangeDefaultSpreadingSpeed(_newSpeed);
            simulation.UpdateAllRegions();
        }
    }

    /// <summary>
    /// Command that changes the disease death probability.
    /// </summary>
    public class ChangeDeathProbabilityCommand : ISimulationCommand
    {
        private readonly double _probability;

        public ChangeDeathProbabilityCommand(double probability)
        {
            _probability = probability;
        }

        /// <summary>Applies new probability and updates regions.</summary>
        public void Execute(Simulation simulation)
        {
            simulation.disease.ChangeDeathProbability(_probability);
            simulation.UpdateAllRegions();
        }
    }

    // Region Commands

    /// <summary>
    /// Command that adds an ability to a region.
    /// </summary>
    public class AddRegionAbilityCommand : ISimulationCommand
    {
        private readonly Region _region;
        private readonly RegionAbility _ability;

        public AddRegionAbilityCommand(Region region, RegionAbility ability)
        {
            _region = region;
            _ability = ability;
        }

        /// <summary>Executes the add ability command.</summary>
        public void Execute(Simulation simulation)
        {
            _region.AddAbility(_ability);
        }
    }

    /// <summary>
    /// Command that removes an ability from a region.
    /// </summary>
    public class RemoveRegionAbilityCommand : ISimulationCommand
    {
        private readonly Region _region;
        private readonly RegionAbility _ability;

        public RemoveRegionAbilityCommand(Region region, RegionAbility ability)
        {
            _region = region;
            _ability = ability;
        }

        /// <summary>Executes the remove ability command.</summary>
        public void Execute(Simulation simulation)
        {
            _region.RemoveAbility(_ability);
        }
    }

    /// <summary>
    /// Command that changes a region's spreading speed.
    /// </summary>
    public class ChangeRegionSpreadingSpeedCommand : ISimulationCommand
    {
        private readonly Region _region;
        private readonly double _newSpeed;

        public ChangeRegionSpreadingSpeedCommand(Region region, double newSpeed)
        {
            _region = region;
            _newSpeed = newSpeed;
        }

        /// <summary>Applies new spreading speed to the region.</summary>
        public void Execute(Simulation simulation)
        {
            _region.ChangeSpreadingSpeed(_newSpeed);
        }
    }

    /// <summary>
    /// Command that changes a region's healthcare index.
    /// </summary>
    public class ChangeRegionHealthcareIndexCommand : ISimulationCommand
    {
        private readonly Region _region;
        private readonly double _newIndex;

        public ChangeRegionHealthcareIndexCommand(Region region, double newIndex)
        {
            _region = region;
            _newIndex = newIndex;
        }

        /// <summary>Applies new healthcare index to the region.</summary>
        public void Execute(Simulation simulation)
        {
            _region.ChangeHealtcareIndex(_newIndex);
        }
    }
}
