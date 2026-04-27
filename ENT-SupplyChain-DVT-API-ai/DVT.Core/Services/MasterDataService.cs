using DVT.Core.Models;
using FluentValidation;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<MasterData> _validator;

        public MasterDataService(IUnitOfWork unitOfWork, IValidator<MasterData> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async ValueTask<IEnumerable<MasterData>> GetAllDivisionsAsync()
        {
            var divisions = await _unitOfWork.MasterData.GetMasterDataByTableNamesAsync(MasterDataTableNames.Division); ;
            if (divisions != null && divisions.Any())
            {
                divisions = divisions.OrderBy(d => d.TextId).OrderBy(d => d.ItemName);
            }
            return divisions;
        }

        public async ValueTask<MasterData> GetByIdAsync(Guid id)
        {
            var masterData = await _unitOfWork.MasterData.GetByIdAsync(id);

            if (masterData == null)
                throw new KeyNotFoundException($"{Constants.StardardMessages.ItemNotFound} id: {id}");

            return masterData;
        }


        public async ValueTask<IEnumerable<string>> GetAllTableNamesAsync()
        {
            var tableNames = await _unitOfWork.MasterData.GetAllTableNamesAsync();
            if (tableNames != null && tableNames.Any())
            {
                tableNames = tableNames.OrderBy(t => t).ToList();
            }
            return tableNames;
        }

        public async ValueTask<IEnumerable<MasterData>> GetMasterDataByTableNamesAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
            var masterData = await _unitOfWork.MasterData.GetMasterDataByTableNamesAsync(tableName);
            if (masterData != null && masterData.Any())
            {
                masterData = masterData.OrderBy(d => d.TableName).OrderBy(d => d.TextId).OrderBy(d => d.ItemName).OrderBy(d => d.ItemNameAbbrev);
            }
            return masterData;
        }

        public async ValueTask<IEnumerable<MasterData>> GetAllMasterDataAsync()
        {
            var masterData = await _unitOfWork.MasterData.GetAllNoTrackingAsync();
            if (masterData != null && masterData.Any())
            {
                masterData = masterData.OrderBy(d => d.TableName).OrderBy(d => d.TextId).OrderBy(d => d.ItemName).OrderBy(d => d.ItemNameAbbrev);
            }
            return masterData;
        }

        public async ValueTask<MasterData> GetMasterDataByIdAsync(Guid id)
        {
            var masterData = await _unitOfWork.MasterData.GetByIdAsync(id);

            if (masterData == null)
                throw new KeyNotFoundException($"{Constants.StardardMessages.ItemNotFound} id: {id}");

            return masterData;
        }
    }
}
