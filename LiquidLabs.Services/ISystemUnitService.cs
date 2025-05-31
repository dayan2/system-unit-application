using LiquidLabs.Dto;

namespace LiquidLabs.Services
{
    public interface ISystemUnitService
    {
        Task<List<SystemUnit>> GetAllSystemUnitsAsync();

        Task<SystemUnit?> GetSystemUnitByIdAsync(int externalId);
    }
}
