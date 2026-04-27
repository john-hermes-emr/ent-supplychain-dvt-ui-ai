using DVT.Core.Models;
using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DVT.Data.Repositories
{
    public class ConfigSettingRepository:Repository<ConfigSetting>, IConfigSettingRepository
    {
        private DVTContext context;

        public ConfigSettingRepository(DVTContext context) : base(context)
        {
            this.context = context;
        }

        public async ValueTask<IEnumerable<ConfigSetting>> GetByModuleAsync(string module)
        {
            return await context.ConfigSettings
                          .OrderByDescending(x => x.Name).Where(x => !x.Deleted && x.Module.Equals(module)).ToListAsync();
        }

        public async ValueTask<ConfigSetting?> GetByModuleAndNameAsync(string module, string name)
        {
            return await context.ConfigSettings
                          .Where(x => !x.Deleted && x.Module.Equals(module) && x.Name.Equals(name)).FirstOrDefaultAsync();
        }
    }
}
