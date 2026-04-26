using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public class Simulation
    {
        public Disease disease { get; set; }
        public Dictionary<int, Region> regions { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly currentSimulationDate { get; set; }
        private bool _isRunning;
        public Queue<UserAction> userActions;
        public int reportingInterval;
        public int dayLength { get; set; }

        public Simulation(Disease disease, Dictionary<int, Region> regions, DateOnly startDate = default)
        {
            this.disease = disease;
            this.regions = regions;
            if (startDate == default)
            {
                this.startDate = DateOnly.FromDateTime(DateTime.Now);
                this.currentSimulationDate = DateOnly.FromDateTime(DateTime.Now);
            }

            this.userActions = new Queue<UserAction>();
            this.reportingInterval = 1;
            this.dayLength = 1000;

        }

        public void Run() { 
            this._isRunning = true;
        }

        public void Stop() {
            this._isRunning = false;
        }

        public bool IsRunning()
        {
            return this._isRunning;
        }

        public void changeDayLength(int ms)
        {
            if (dayLength < 0)
                this.dayLength = ms;
            else
                Console.WriteLine("Neplatný čas");
        }

        public async Task Simulate(CancellationToken ct)
        {
            while (this._isRunning && !ct.IsCancellationRequested)
            {
                currentSimulationDate = currentSimulationDate.AddDays(1);
                string dayString = currentSimulationDate.ToString() + "\n";

                dayString += SimulateDay().ToString();
                   
                Console.WriteLine(dayString);

                try
                {
                    await Task.Delay(dayLength, ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private StatisticUpdate SimulateDay()
        {
            int newSick = 0;
            int newDead = 0;
            int newVaccinated = 0;

            bool updateAllRegions = false;
            while (userActions.Count > 0)
            {
                UserAction action = userActions.Dequeue();

                switch (action.actionType)
                {
                    case (UserAction.ActionType.AddDiseaseAbility):
                        if (action.ability != null && action.ability is DiseaseAbility diseaseAddedAbility)
                        {
                            this.disease.addAbility(diseaseAddedAbility);
                            updateAllRegions = true;
                        }

                        break;

                    case (UserAction.ActionType.RemoveDiseaseAbility):
                        if (action.ability != null && action.ability is DiseaseAbility diseaseRemovedAbility)
                        {
                            this.disease.removeAbility(diseaseRemovedAbility);
                            updateAllRegions = true;
                        }
                        break;


                    case (UserAction.ActionType.ChangeDieseaseSpreadingSpeed):
                        if (action.doubleValue != null)
                        {
                            this.disease.changeDefaultSpreadingSpeed((double)action.doubleValue);
                            updateAllRegions = true;
                        }
                        break;

                    case (UserAction.ActionType.AddRegionAbility):
                        if (action.ability != null && action.ability is RegionAbility regionAddedAbility && action.changedRegion != null)
                            this.regions[action.changedRegion.id].addAbility(regionAddedAbility);
                        break;

                    case (UserAction.ActionType.RemoveRegionAbility):
                        if (action.ability != null && action.ability is RegionAbility regionRemovedAbility && action.changedRegion != null)
                            this.regions[action.changedRegion.id].removeAbility(regionRemovedAbility);
                        break;

                    case (UserAction.ActionType.ChangeRegionHealthcareIndex):
                        if (action.changedRegion != null && action.doubleValue != null)
                            this.regions[action.changedRegion.id].changeHealtcareIndex((double)action.doubleValue);
                        break;


                    default:
                        Console.WriteLine("Neznama uzivatelska akce: " + action.actionType);
                        break;
                }
            }

            foreach (int key in this.regions.Keys)
            {
                if (updateAllRegions)
                    regions[key].updateSpreadingSpeed();

                StatisticUpdate regionUpdate = regions[key].simulateDay();
                newDead += regionUpdate.newDead;
                newSick += regionUpdate.newSick;
                newVaccinated += regionUpdate.newVaccinated;

            }

            return new StatisticUpdate(newSick, newDead, newVaccinated);
        }
    }
}
