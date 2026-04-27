using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT.Core.Services
{
    public class DbHealthCheckService:IDbHealthCheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DbHealthCheckService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> TestDatabaseConnectionAsync()
        {
            return await _unitOfWork.TestDatabaseConnectionAsync();
        }

        public async Task<List<string>> GetDatabaseTablesAsync()
        {
            return await _unitOfWork.GetListOfTables();
        }
    }

    public interface IDbHealthCheckService
    {
        Task<bool> TestDatabaseConnectionAsync();
        Task<List<string>> GetDatabaseTablesAsync();
    }
}
