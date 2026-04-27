using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IMasterDataService
    {
        ValueTask<IEnumerable<MasterData>> GetAllDivisionsAsync();
        ValueTask<MasterData> GetByIdAsync(Guid id);

        ValueTask<IEnumerable<string>> GetAllTableNamesAsync();
        ValueTask<IEnumerable<MasterData>> GetAllMasterDataAsync();

        ValueTask<IEnumerable<MasterData>> GetMasterDataByTableNamesAsync(string tableName);
        ValueTask<MasterData> GetMasterDataByIdAsync(Guid id);
    }
}
