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
        public RegionAbility RegionAbility { get; set; }
        public DiseaseAbility DiseaseAbility { get; set; }
        public double SecondaryModifier { get; set; }
        public Interaction(RegionAbility regionAbility, DiseaseAbility diseaseAbility, double secondaryModifier)
        {
            RegionAbility = regionAbility;
            DiseaseAbility = diseaseAbility;
            SecondaryModifier = secondaryModifier;
        }
    }
}
