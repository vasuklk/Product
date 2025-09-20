using System.Threading.Tasks;

namespace ProductAPI.Helpers
{
    public interface IUniqueIdGenerator
    {
        Task<int> GenerateUniqueIdAsync();
        bool IsValidId(int id);
    }
}