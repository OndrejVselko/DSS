using SimulationCore;

namespace Data
{
    /// <summary>
    /// Factory interface for creating abilities from DTOs.
    /// </summary>
    public interface IAbilityFactory
    {
        /// <summary>
        /// Create an IAbility instance from the given DTO.
        /// </summary>
        /// <param name="dto">Source DTO</param>
        /// <returns>Created ability</returns>
        IAbility Create(AbilityDto dto);
    }
}