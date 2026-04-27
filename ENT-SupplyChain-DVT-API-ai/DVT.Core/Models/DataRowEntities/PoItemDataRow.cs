using DVT.Core.Helper;
using static DVT.Core.Constants;

namespace DVT.Core.Models.DataRowEntities
{
    public class POItemDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionID { get; set; }
        public string LocalSiteID { get; set; }
        public string PONumber { get; set; }
        public string POLineNumber { get; set; }
        public string PartNumber { get; set; }
        public string SupplierPartNumber { get; set; }
        public string Description { get; set; }
        public string ContractID { get; set; }
        public string UnitCostOriginalStr { get; set; }
        public BigDecimal? UnitCost { get; set; }
        public ErrorTypes UnitCostError { get; set; } = ErrorTypes.None;
        public string PureLoadedCost { get; set; }
        public string OrderedValueOriginalStr { get; set; }
        public BigDecimal? OrderedValue { get; set; }
        public ErrorTypes OrderedValueError { get; set; } = ErrorTypes.None;
        public string QuantityOrderedOriginalStr { get; set; }
        public BigDecimal? QuantityOrdered { get; set; }
        public ErrorTypes QuantityOrderedError { get; set; } = ErrorTypes.None;
        public string QuantityReturnedOriginalStr { get; set; }
        public BigDecimal? QuantityReturned { get; set; }
        public ErrorTypes QuantityReturnedError { get; set; } = ErrorTypes.None;
        public DateTime? CommittedDate { get; set; }
        public string CommittedDateStr { get; set; }
        public string CommittedDateError { get; set; } = "";
        public DateTime? RequestedDate { get; set; }
        public string RequestedDateStr { get; set; }
        public string RequestedDateError { get; set; } = "";
        public string OrderStatus { get; set; }
        public string CurrencyCode { get; set; }
        public string UOM { get; set; }
        public string QtyLeftToReceiveOriginalStr { get; set; }
        public BigDecimal? QtyLeftToReceive { get; set; }
        public ErrorTypes QtyLeftToReceiveError { get; set; } = ErrorTypes.None;
        public string ValueLeftToReceiveOriginalStr { get; set; }
        public BigDecimal? ValueLeftToReceive { get; set; }
        public ErrorTypes ValueLeftToReceiveError { get; set; } = ErrorTypes.None;
        public string ReleaseOriginalStr { get; set; }
        public BigDecimal? Release { get; set; }
        public ErrorTypes ReleaseError { get; set; } = ErrorTypes.None;

        private string _uniquenessKey = "";

        //Ignore this property for entity framework mapping, as it's only used for in-memory duplicate checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UniquenessKey
        {
            get { return _uniquenessKey; }
        }

        public void GenerateUniquenessKey()
        {
            //|DIVISION ID|+|LOCAL SITE ID|+|PO NUMBER|+|PO LINE NUMBER|+|PART NUMBER|+|COMMITTED DATE|+|REQUESTED DATE|+|RELEASE#|

            //Check for null on dependent fields if null, return empty string
            if (string.IsNullOrWhiteSpace(DivisionID) || string.IsNullOrWhiteSpace(LocalSiteID) || string.IsNullOrWhiteSpace(PONumber) 
                || string.IsNullOrWhiteSpace(POLineNumber) || string.IsNullOrWhiteSpace(CommittedDateStr) || string.IsNullOrWhiteSpace(RequestedDateStr) 
                || string.IsNullOrWhiteSpace(ReleaseOriginalStr))
            {
                _uniquenessKey = "";
                return;
            }

            _uniquenessKey = $"{DivisionID.ToLower()}_{LocalSiteID.ToLower()}_{PONumber.ToLower()}_{POLineNumber.ToLower()}_{CommittedDateStr.ToLower()}_{RequestedDateStr.ToLower()}_{ReleaseOriginalStr.ToLower()}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; } 
    }
}
