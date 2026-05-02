using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Simulation
    {
        public event Action<string>? OnDaySimulated;

        public Disease disease { get; set; }
        public Dictionary<int, Region> regions { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly currentSimulationDate { get; set; }
        private bool _isRunning;
        public Queue<UserAction> userActions;
        public int reportingInterval;
        public int dayLength { get; set; }

        public Simulation()
        {
            this.userActions = new Queue<UserAction>();
            this.reportingInterval = 1;
            this.dayLength = 1000;
        }

        public void SetDisease(Disease disease)
        {
            this.disease = disease;
        }

        public void SetRegions(List<Region> regions)
        {
            this.regions = new Dictionary<int, Region>();
            foreach (var region in regions)
            {
                this.regions[region.Id] = region;
            }
        }

        public void SetStartDate(DateOnly startDate = default)
        {
            if (startDate == default)
            {
                this.startDate = DateOnly.FromDateTime(DateTime.Now);
                this.currentSimulationDate = DateOnly.FromDateTime(DateTime.Now);
            }
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

        public void SetRegionQueues()
        {
            foreach (int key in this.regions.Keys)
            {
                regions[key].SetStartingQueue(this.disease.SicknessLength);
            }
        }

        public void UpdateRegionsDiseaseValues()
        {
            foreach (var key in this.regions.Keys)
            {
                regions[key].UpdateDiseaseValues(disease.TotalSpreadingSpeed, disease.DeathProbability);
            }
        }

        public void changeDayLength(int ms)
        {
            if (dayLength > 0)
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

                OnDaySimulated?.Invoke(dayString);

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
            int totalSick = 0;
            int totalDeath = 0;
            int totalVaccinated = 0;
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
                            this.disease.AddAbility(diseaseAddedAbility);
                            updateAllRegions = true;
                        }
                        break;

                    case (UserAction.ActionType.RemoveDiseaseAbility):
                        if (action.ability != null && action.ability is DiseaseAbility diseaseRemovedAbility)
                        {
                            this.disease.RemoveAbility(diseaseRemovedAbility);
                            updateAllRegions = true;
                        }
                        break;

                    case (UserAction.ActionType.ChangeDefaultSpreadingSpeed):
                        this.disease.ChangeDefaultSpreadingSpeed((double)action.doubleValue!);
                        updateAllRegions = true;
                        break;

                    case (UserAction.ActionType.AddRegionAbility):
                        if (action.ability != null && action.ability is RegionAbility regionAddedAbility && action.changedRegion != null)
                            this.regions[action.changedRegion.Id].AddAbility(regionAddedAbility);
                        break;

                    case (UserAction.ActionType.RemoveRegionAbility):
                        if (action.ability != null && action.ability is RegionAbility regionRemovedAbility && action.changedRegion != null)
                            this.regions[action.changedRegion.Id].RemoveAbility(regionRemovedAbility);
                        break;

                    case (UserAction.ActionType.ChangeRegionHealthcareIndex):
                        if (action.changedRegion != null && action.doubleValue != null)
                            this.regions[action.changedRegion.Id].ChangeHealtcareIndex((double)action.doubleValue);
                        break;

                    case (UserAction.ActionType.ChangeDeathProbability):
                        this.disease.ChangeDeathProbability((double)action.doubleValue!);
                        updateAllRegions = true;
                        break;
                    default:
                        Console.WriteLine("Neznama uzivatelska akce: " + action.actionType);
                        break;
                }
            }

            foreach (int key in this.regions.Keys)
            {
                if (updateAllRegions)
                {
                    regions[key].UpdateDiseaseValues(disease.TotalSpreadingSpeed, disease.DeathProbability);
                    regions[key].UpdateRegionValues();
                }

                StatisticUpdate regionUpdate = regions[key].SimulateDay();
                newDead += regionUpdate.NewDead;
                newSick += regionUpdate.NewSick;
                newVaccinated += regionUpdate.NewVaccinated;
                totalSick += regionUpdate.TotalSick;
                totalDeath += regionUpdate.TotalDead;
                totalVaccinated += regionUpdate.TotalVaccinated;

            }
            updateAllRegions = false;
            return new StatisticUpdate(newSick, newDead, newVaccinated, totalSick, totalDeath, totalVaccinated);
        }
    }
}
