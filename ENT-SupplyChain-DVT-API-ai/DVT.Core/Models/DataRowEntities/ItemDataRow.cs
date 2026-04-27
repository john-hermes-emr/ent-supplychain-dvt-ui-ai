using DVT.Core.Helper;
using static DVT.Core.Constants;

namespace DVT.Core.Models.DataRowEntities
{
    public class ItemDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionId { get; set; }
        public string LocalSiteId { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Comcode { get; set; }
        public string DRICode { get; set; }
        public string PartStatus { get; set; }
        public string DirectIndirect { get; set; }
        public string PurchMfrd { get; set; }
        public BigDecimal? LeadTime { get; set; }
        public ErrorTypes LeadTimeError { get; set; } = ErrorTypes.None;
        public string LeadTimeOriginalStr { get; set; }
        public string StandardCostOriginalStr { get; set; }
        public BigDecimal? StandardCost { get; set; }
        public ErrorTypes StandardCostError { get; set; } = ErrorTypes.None;
        public string PureLoadedCost { get; set; }
        public string CurrencyCode { get; set; }
        public string UOM { get; set; }
        public string ABCCategory { get; set; }
        public BigDecimal? ItemWeight { get; set; }
        public ErrorTypes ItemWeightError { get; set; } = ErrorTypes.None;
        public string ItemWeightOriginalStr { get; set; }
        public string ItemWeightUOM { get; set; }
        public string ItemHtsCode { get; set; }
        public string ItemHsCode { get; set; }

        private string _uniquenessKey = "";

        //Ignore this property for entity framework mapping, as it's only used for in-memory duplicate checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UniquenessKey
        {
            get { return _uniquenessKey; }
        }

        public void GenerateUniquenessKey()
        {
            //Check for null on dependent fields if null, return empty string
            if (string.IsNullOrWhiteSpace(DivisionId) || string.IsNullOrWhiteSpace(LocalSiteId) || string.IsNullOrWhiteSpace(PartNumber))
            {
                _uniquenessKey = "";
                return;
            }

            _uniquenessKey = $"{DivisionId.ToLower()}_{LocalSiteId.ToLower()}_{PartNumber.ToLower()}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }
    }
}
