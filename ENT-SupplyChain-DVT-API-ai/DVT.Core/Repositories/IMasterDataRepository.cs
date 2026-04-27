using DVT.Core.Models;

namespace DVT.Core.Repositories
{
    public interface IMasterDataRepository: IRepository<MasterData>
    {
        ValueTask<IEnumerable<string>> GetAllTableNamesAsync();
        ValueTask<IEnumerable<MasterData>> GetMasterDataByTableNamesAsync(string tableName);

        ValueTask<MasterDataValidationResult> VerifyDivision(List<string> divisionIds);
        ValueTask<MasterDataValidationResult> VerifySite(List<string> siteIds);
        ValueTask<MasterDataValidationResult> VerifyUOM(List<string> uomIds);
        ValueTask<MasterDataValidationResult> VerifyCurrency(List<string> currencyIds);
        ValueTask<MasterDataValidationResult> VerifyCommodityCode(List<string> commodityCodeIds);
        ValueTask<MasterDataValidationResult> VerifyFreightTerms(List<string> freightTermsIds);
        ValueTask<MasterDataValidationResult> VerifyPaymentTerms(List<string> paymentTermIds);

    }
}
