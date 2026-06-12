using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Represents an interaction between a disease ability and a region ability.
    /// </summary>
    public class Interaction
    {
        public int DiseaseAbilityId { get; set; }
        public int RegionAbilityId { get; set; }
        public double SpreadingModifier { get; set; }
        public double DeathModifier { get; set; }
        public string Comment { get; set; }

        public Interaction(int diseaseAbilityId, int regionAbilityId, double spreadingModifier, double deathModifier, string comment)
        {
            DiseaseAbilityId = diseaseAbilityId;
            RegionAbilityId = regionAbilityId;
            SpreadingModifier = spreadingModifier;
            DeathModifier = deathModifier;
            Comment = comment;
        }

    }
}
