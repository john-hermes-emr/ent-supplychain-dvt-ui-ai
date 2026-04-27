using DVT.Core;
using DVT.Core.Repositories;
using DVT.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel;

namespace DVT.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DVTContext _context;
        private UserInfoRepository userInfoRepository;
        private ActivityLogRepository activityLogRepository;
        private JobRepository jobRepository;
        private JobFileRepository jobFileRepository;
        private MasterDataRepository masterDataRepository;
        private ConfigSettingRepository configSettingRepository;

        public IUserInfoRepository UserInfos => userInfoRepository ??= new UserInfoRepository(_context);
        public IActivityLogRepository ActivityLogs => activityLogRepository ??= new ActivityLogRepository(_context);
        public IJobRepository Jobs => jobRepository ??= new JobRepository(_context);
        public IJobFileRepository JobFiles => jobFileRepository ??= new JobFileRepository(_context);
        public IMasterDataRepository MasterData=> masterDataRepository ??= new MasterDataRepository(_context);
        public IConfigSettingRepository ConfigSettings => configSettingRepository ??= new ConfigSettingRepository(_context);

        public UnitOfWork(DVTContext context)
        {
            this._context = context;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async ValueTask<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async ValueTask<List<string>> GetListOfTables()
        {
            List<string> tableNames = new List<string>();
            var connection = _context.Database.GetDbConnection();            

            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog', 'information_schema') AND table_type = 'BASE TABLE' ORDER BY table_schema, table_name;";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tableNames.Add(reader.GetString(1));
                        }
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return tableNames;
        }
        
        public async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}