using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Vaccine
    {
        public double ProtectionEfficiency {  get; set; }
        public double DeathProtectionEfficiency { get; set; }

        public Vaccine(double protectionEfficiency, double deathProtectionEfficiency) {
            ProtectionEfficiency = protectionEfficiency;
            DeathProtectionEfficiency = deathProtectionEfficiency;
        }

        public void ChangeVaccineEfficiency(double? protectionEfficiency, double? deathProtectionEfficiency)
        {
            if( protectionEfficiency.HasValue)
                ProtectionEfficiency = (double)protectionEfficiency;

            if( deathProtectionEfficiency.HasValue)
                DeathProtectionEfficiency = (double)deathProtectionEfficiency;
        }

    }
}
