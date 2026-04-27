using DVT.Core.Models;
using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using static DVT.Core.Constants;

namespace DVT.Data.Repositories
{
    public class MasterDataRepository : Repository<MasterData>, IMasterDataRepository
    {
        private DVTContext _context;

        public MasterDataRepository(DVTContext context) : base(context)
        {
            this._context = context;
        }

        public async ValueTask<MasterData> GetByIdAsync(Guid id)
        {
            return await _context.MasterData
                .SingleOrDefaultAsync(x => x.ItemId == id && !x.Deleted);
        }

        public async ValueTask<IEnumerable<string>> GetAllTableNamesAsync()
        {
            return await _context.MasterData
                .Where(md => !md.Deleted)
                .Select(md => md.TableName)
                .Distinct()
                .ToListAsync();
        }

        public async ValueTask<IEnumerable<MasterData>> GetMasterDataByTableNamesAsync(string tableName)
        {
            return await _context.MasterData
                .Where(md => !md.Deleted && string.Equals(md.TableName, tableName))
                .ToListAsync();
        }

        public async ValueTask<MasterDataValidationResult> VerifyDivision(List<string> divisionIds)
        {
            var validDivisions = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.Division) && divisionIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validDivisions.Count == divisionIds.Count,
                InvalidIds = divisionIds.Except(validDivisions.Select(md => md.TextId)).ToList()
            };
        }

        public async ValueTask<MasterDataValidationResult> VerifySite(List<string> siteIds)
        {
            var validSites = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.SiteMaster) && siteIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validSites.Count == siteIds.Count,
                InvalidIds = siteIds.Except(validSites.Select(md => md.TextId)).ToList()
            };
        }

        public async ValueTask<MasterDataValidationResult> VerifyUOM(List<string> uomIds)
        {
            var validUOMs = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.UOM) && uomIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validUOMs.Count == uomIds.Count,
                InvalidIds = uomIds.Except(validUOMs.Select(md => md.TextId)).ToList()
            };
        }

        public async ValueTask<MasterDataValidationResult> VerifyCurrency(List<string> currencyIds)
        {
            var validCurrencies = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.Currency) && currencyIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validCurrencies.Count == currencyIds.Count,
                InvalidIds = currencyIds.Except(validCurrencies.Select(md => md.TextId)).ToList()
            };
        }

        public async ValueTask<MasterDataValidationResult> VerifyCommodityCode(List<string> commodityCodeIds)
        {
            var validCommodityCodes = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.CommodityCode) && commodityCodeIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validCommodityCodes.Count == commodityCodeIds.Count,
                InvalidIds = commodityCodeIds.Except(validCommodityCodes.Select(md => md.TextId)).ToList()
            };
        }

        public async ValueTask<MasterDataValidationResult> VerifyFreightTerms(List<string> freightTermsIds)
        {
            var validFreightTerms = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.FreightTerms) && freightTermsIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validFreightTerms.Count == freightTermsIds.Count,
                InvalidIds = freightTermsIds.Except(validFreightTerms.Select(md => md.TextId)).ToList()
            };
        }

        public async ValueTask<MasterDataValidationResult> VerifyPaymentTerms(List<string> paymentTermIds)
        {
            var validPaymentTerms = await _context.MasterData
                .Where(md => !md.Deleted && md.TableName.Equals(MasterDataTableNames.PaymentTerm) && paymentTermIds.Contains(md.TextId))
                .ToListAsync();

            return new MasterDataValidationResult
            {
                IsValid = validPaymentTerms.Count == paymentTermIds.Count,
                InvalidIds = paymentTermIds.Except(validPaymentTerms.Select(md => md.TextId)).ToList()
            };
        }
    }
}
