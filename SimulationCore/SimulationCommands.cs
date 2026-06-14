using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

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

            var relevantInteractions = simulation.Interactions
                .Where(kvp => kvp.Key.Item1 == _ability.Id)
                .Select(kvp => kvp.Value);

            foreach (Region region in simulation.regions.Values)
            {
                foreach (Interaction interaction in relevantInteractions)
                    if (region.Abilities.Any(a => a.Id == interaction.RegionAbilityId))
                        region.AddInteraction(interaction);
            }

            simulation.AddLog(simulation.currentSimulationDate, "Přidána vlastnost nemoci", _ability.Name);

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

            var relevantInteractions = simulation.Interactions
                .Where(kvp => kvp.Key.Item1 == _ability.Id)
                .Select(kvp => kvp.Value);

            foreach (Region region in simulation.regions.Values)
            {
                foreach (Interaction interaction in relevantInteractions)
                {
                    if (region.ActiveInteractions.Contains(interaction))
                        region.RemoveInteraction(interaction);
                }
            }

            simulation.AddLog(simulation.currentSimulationDate, "Odebrána vlastnost nemoci", _ability.Name);

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
            simulation.AddLog(simulation.currentSimulationDate, "Změna rychlosti šíření", _newSpeed.ToString());

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

            simulation.AddLog(simulation.currentSimulationDate, "Změna šance na úmrtí", (_probability * 100).ToString());

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
            var relevantInteractions = simulation.Interactions
             .Where(kvp => kvp.Key.Item2 == _ability.Id)
             .Select(kvp => kvp.Value);

            simulation.AddLog(simulation.currentSimulationDate, "Přidána vlastnost regionu", _region.Name, _ability.Name);

            foreach (var interaction in relevantInteractions)
            {
                if (simulation.disease.Abilities.Any(a => a.Id == interaction.DiseaseAbilityId))
                {
                    simulation.AddLog(simulation.currentSimulationDate, _region.Name + " - nová interakce: " + _ability.Name + " + " +simulation.disease.Abilities[interaction.DiseaseAbilityId].Name + ", Modifikátory: " + interaction.SpreadingModifier + "|" + interaction.DeathModifier);
                    _region.AddInteraction(interaction);
                }
            }
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

            var relevantInteractions = simulation.Interactions
             .Where(kvp => kvp.Key.Item2 == _ability.Id)
             .Select(kvp => kvp.Value);

            foreach (var interaction in relevantInteractions)
            {
                _region.RemoveInteraction(interaction);
            }

            simulation.AddLog(simulation.currentSimulationDate, "Odebrána vlastnost regionu", _region.Name, _ability.Name);
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
            simulation.AddLog(simulation.currentSimulationDate, "Změna rychlosti šíření regionu", _region.Name, _newSpeed.ToString());
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
            simulation.AddLog(simulation.currentSimulationDate, "Změna indexu zdravotnictví", _region.Name, _newIndex.ToString());
        }
    }

    // Vaccine commands

    public class ChangeVaccineParametersCommand : ISimulationCommand { 
        private readonly double? _newProtecitonEfficiency;
        private readonly double? _newDeathProtecitonEfficiency;

        
        public ChangeVaccineParametersCommand(double? newProtecitonEfficiency, double? newDeathProtecitonEfficiency)
        {
            _newDeathProtecitonEfficiency = newDeathProtecitonEfficiency;
            _newProtecitonEfficiency = newProtecitonEfficiency;
        }

        public void Execute(Simulation simulation)
        {
            simulation.vaccine.ChangeVaccineEfficiency(_newProtecitonEfficiency, _newDeathProtecitonEfficiency);
            simulation.AddLog(simulation.currentSimulationDate, "Změna parametrů vakcíny",
                (_newProtecitonEfficiency * 100)?.ToString() ?? "-",
                (_newDeathProtecitonEfficiency * 100)?.ToString() ?? "-");
        }
    }

    public class StartVaccinationSingleRegionCommand : ISimulationCommand
    {
        private readonly Region _region;

        public StartVaccinationSingleRegionCommand(Region region)
        {
            _region = region;
        }

        public void Execute(Simulation simulation)
        {
            _region.StartVaccinating();
            simulation.AddLog(simulation.currentSimulationDate, "Spuštěno očkování", _region.Name);
        }
    }

    public class StopVaccinationSingleRegionCommand : ISimulationCommand
    {
        private readonly Region _region;


        public StopVaccinationSingleRegionCommand(Region region)
        {
            _region = region;
        }

        public void Execute(Simulation simulation)
        {
            _region.StopVaccinating();
            simulation.AddLog(simulation.currentSimulationDate, "Zastaveno očkování", _region.Name);
        }
    }

    public class StartVaccinationAllRegionCommand : ISimulationCommand
    {

        public StartVaccinationAllRegionCommand()
        {
        }

        public void Execute(Simulation simulation)
        {
            foreach (int key in simulation.regions.Keys)
            {
                simulation.regions[key].StartVaccinating();
            }
            simulation.AddLog(simulation.currentSimulationDate, "Spuštěno očkování ve všech regionech");

        }
    }

    public class StopVaccinationAllRegionCommand : ISimulationCommand
    {
        private readonly Region _region;

        public StopVaccinationAllRegionCommand()
        {
        }

        public void Execute(Simulation simulation)
        {
            foreach (int key in simulation.regions.Keys)
            {
                simulation.regions[key].StopVaccinating();
            }
            simulation.AddLog(simulation.currentSimulationDate, "Zastaveno očkování ve všech regionech");
        }
    }


    // Simulation commands
    public class ChangeSimulationSpeedCommand : ISimulationCommand
    {
        private readonly int _newSpeed;
        public ChangeSimulationSpeedCommand(int newSpeed)
        {
            _newSpeed = newSpeed;
        }

        public void Execute(Simulation simulation)
        {
            simulation.changeDayLength(_newSpeed);
            simulation.AddLog(simulation.currentSimulationDate, "Změna rychlosti simulace", _newSpeed.ToString());
        }
    }
}
