using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore
{
    /// <summary>
    /// Represents an interaction between a disease ability and a region ability.
    /// </summary>
    public class Interaction
    {
        /// <summary>Id of the disease ability.</summary>
        public int DiseaseAbilityId { get; set; }
        /// <summary>Id of the region ability.</summary>
        public int RegionAbilityId { get; set; }
        /// <summary>Secondary modifier applied when both abilities are present.</summary>
        public double SecondaryModifier { get; set; }
        /// <summary>Optional comment describing the interaction.</summary>
        public string Comment { get; set; }

        /// <summary>
        /// Initializes a new interaction instance.
        /// </summary>
        public Interaction(int diseaseAbilityId, int regionAbilityId, double secondaryModifier, string comment)
        {
            DiseaseAbilityId = diseaseAbilityId;
            RegionAbilityId = regionAbilityId;
            SecondaryModifier = secondaryModifier;
            Comment = comment;
        }
    }
}
