using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DVT.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IUserInfoRepository UserInfos { get; }
        IActivityLogRepository ActivityLogs { get; }
        IJobRepository Jobs { get; }
        IJobFileRepository JobFiles { get; }
        IMasterDataRepository MasterData { get; }
        ValueTask<IDbContextTransaction> BeginTransactionAsync();
        Task<int> CommitAsync();
        Task<bool> TestDatabaseConnectionAsync();
        ValueTask<List<string>> GetListOfTables();
        IConfigSettingRepository ConfigSettings { get; }
    }
}