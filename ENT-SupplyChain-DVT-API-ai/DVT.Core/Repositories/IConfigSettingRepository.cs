using DVT.Core.Models;

namespace DVT.Core.Repositories
{
    public interface IConfigSettingRepository : IRepository<ConfigSetting>
    {
        ValueTask<IEnumerable<ConfigSetting>> GetByModuleAsync(string module);
        ValueTask<ConfigSetting?> GetByModuleAndNameAsync(string module, string name);
    }
}
