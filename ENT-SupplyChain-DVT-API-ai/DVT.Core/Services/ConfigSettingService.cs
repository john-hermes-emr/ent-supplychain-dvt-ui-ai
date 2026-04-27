using DVT.Core.Models;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class ConfigSettingService : IConfigSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _environmentName;
        public ConfigSettingService(IUnitOfWork unitOfWork, string environmentName) 
        {
            _unitOfWork = unitOfWork;
            _environmentName = environmentName;
        }
        public async Task<IEnumerable<ConfigSetting>> GetHelpDocumentsAsync()
        {
            return await _unitOfWork.ConfigSettings.GetByModuleAsync(ConfigSettingModules.HelpDocuments);
        }

        public async ValueTask<ConfigSetting?> GetSettingByModuleAndNameAsync(string moduleName, string settingName)
        {
            return await _unitOfWork.ConfigSettings.GetByModuleAndNameAsync(moduleName, settingName + _environmentName);
        }
    }
}
