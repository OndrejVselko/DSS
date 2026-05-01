using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public struct UserAction
    {
        public enum ActionType
        {
            ChangeDefaultSpreadingSpeed,
            AddDiseaseAbility,
            RemoveDiseaseAbility,
            ChangeRegionHealthcareIndex,
            AddRegionAbility,
            RemoveRegionAbility,
            ChangeDeathProbability
        }

        public ActionType actionType;
        public Region? changedRegion;
        public IAbility? ability;
        public double? doubleValue;


        public UserAction(ActionType actionType, double? doubleValue = null, IAbility? ability = null, Region? changedRegion = null) {
            this.actionType = actionType;
            if (doubleValue != null)
                this.doubleValue = doubleValue;
            if (ability != null)
                this.ability = ability;
            if (changedRegion != null)
                this.changedRegion = changedRegion;
        }
    }
}
