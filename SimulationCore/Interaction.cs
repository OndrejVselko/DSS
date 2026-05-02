using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    public class Interaction
    {
        public int DiseaseAbilityId { get; set; }
        public int RegionAbilityId { get; set; }
        public double SecondaryModifier { get; set; }
        public string Comment { get; set; }

        public Interaction(int diseaseAbilityId, int regionAbilityId, double secondaryModifier, string comment)
        {
            DiseaseAbilityId = diseaseAbilityId;
            RegionAbilityId = regionAbilityId;
            SecondaryModifier = secondaryModifier;
            Comment = comment;
        }
    }
}
