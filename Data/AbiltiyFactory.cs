using SimulationCore;
using Shared;

namespace Data
{
    /// <summary>
    /// Factory that creates disease-specific abilities.
    /// </summary>
    public class DiseaseAbilityFactory : IAbilityFactory
    {
        /// <summary>
        /// Create a DiseaseAbility from a DTO.
        /// </summary>
        public IAbility Create(AbilityDto dto) =>
            new DiseaseAbility(dto.Id, dto.Name, dto.Description, dto.SpreadingModifier, dto.DeathModifier, dto.BorderModifier, dto.VaccinationCapacityModifier);
    }

    /// <summary>
    /// Factory that creates region-specific abilities.
    /// </summary>
    public class RegionAbilityFactory : IAbilityFactory
    {
        /// <summary>
        /// Create a RegionAbility from a DTO.
        /// </summary>
        public IAbility Create(AbilityDto dto) =>
            new RegionAbility(dto.Id, dto.Name, dto.Description, dto.SpreadingModifier, dto.DeathModifier, dto.BorderModifier, dto.VaccinationCapacityModifier);
    }

    /// <summary>
    /// Central factory that exposes typed creation helpers for abilities.
    /// </summary>
    public class AbilityFactory
    {
        /// <summary>
        /// Internal registry of sub-factories keyed by type name.
        /// </summary>
        private readonly Dictionary<string, IAbilityFactory> _factories;

        /// <summary>
        /// Initializes the factory registry.
        /// </summary>
        public AbilityFactory()
        {
            _factories = new Dictionary<string, IAbilityFactory>
            {
                ["disease"] = new DiseaseAbilityFactory(),
                ["region"] = new RegionAbilityFactory(),
            };
        }

        /// <summary>
        /// Create a DiseaseAbility using the disease sub-factory.
        /// </summary>
        public DiseaseAbility CreateDisease(AbilityDto dto) =>
            (DiseaseAbility)_factories["disease"].Create(dto);

        /// <summary>
        /// Create a RegionAbility using the region sub-factory.
        /// </summary>
        public RegionAbility CreateRegion(AbilityDto dto) =>
            (RegionAbility)_factories["region"].Create(dto);
    }
}