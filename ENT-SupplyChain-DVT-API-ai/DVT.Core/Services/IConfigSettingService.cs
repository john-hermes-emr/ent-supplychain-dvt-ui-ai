using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IConfigSettingService
    {
        Task<IEnumerable<ConfigSetting>> GetHelpDocumentsAsync();
        ValueTask<ConfigSetting?> GetSettingByModuleAndNameAsync(string moduleName, string settingName);
    }
}
