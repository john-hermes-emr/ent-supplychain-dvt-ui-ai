using DVT.Core.Helper;
using static DVT.Core.Constants;

namespace DVT.Core.Models.DataRowEntities
{
    public class InventoryDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionId { get; set; }
        public string LocalSiteId { get; set; }
        public string PartNumber { get; set; }
        public string QuantityOriginalStr { get; set; }
        public BigDecimal? Quantity { get; set; }
        public ErrorTypes QuantityError { get; set; } = ErrorTypes.None;
        public string StandardCostOriginalStr { get; set; }
        public BigDecimal? StandardCost { get; set; }
        public ErrorTypes StandardCostError { get; set; } = ErrorTypes.None;
        public string TotalValueOriginalStr { get; set; }
        public BigDecimal? TotalValue { get; set; }
        public ErrorTypes TotalValueError { get; set; } = ErrorTypes.None;
        public string UOM { get; set; }
        public string CurrencyCode { get; set; }
        public string PartStatus { get; set; }
        public string Comcode { get; set; }
        public string DRICode { get; set; }
        public string Description { get; set; }
        public DateTime? InventoryDate { get; set; }
        public string InventoryDateStr { get; set; }
        public string InventoryDateError { get; set; } = "";

        private string _uniquenessKey = "";

        //Ignore this property for entity framework mapping, as it's only used for in-memory duplicate checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UniquenessKey
        {
            get { return _uniquenessKey; }
        }

        public void GenerateUniquenessKey()
        {
            //Check for null on the individual fields that are part of the key and generate an empty uniqueness key
            if (string.IsNullOrWhiteSpace(DivisionId) || string.IsNullOrWhiteSpace(LocalSiteId) || string.IsNullOrWhiteSpace(PartNumber) || string.IsNullOrWhiteSpace(InventoryDateStr))
            {
                _uniquenessKey = "";
                return;
            }

            _uniquenessKey = $"{DivisionId.ToLower()}_{LocalSiteId.ToLower()}_{PartNumber.ToLower()}_{InventoryDateStr}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }
    }
}
