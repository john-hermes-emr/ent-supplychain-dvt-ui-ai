using DVT.Core.Helper;
using static DVT.Core.Constants;

namespace DVT.Core.Models.DataRowEntities
{
    public class VirDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionId { get; set; }
        public string LocalSiteId { get; set; }
        public string ReceiptNumber { get; set; }
        public string PoNumber { get; set; }
        public string POLineNumber { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public string SupplierPartNumber { get; set; }
        public string QuantityOrderedOriginalStr { get; set; }
        public BigDecimal? QuantityOrdered { get; set; }
        public ErrorTypes QuantityOrderedError { get; set; } = ErrorTypes.None;
        public string QuantityReceivedOriginalStr { get; set; }
        public BigDecimal? QuantityReceived { get; set; }
        public ErrorTypes QuantityReceivedError { get; set; } = ErrorTypes.None;
        public DateTime? DateReceived { get; set; }
        public string DateReceivedStr { get; set; }
        public string DateReceivedError { get; set; } = "";
        public string InvoicePricePaidOriginalStr { get; set; }
        public BigDecimal? InvoicePricePaid { get; set; }
        public ErrorTypes InvoicePricePaidError { get; set; } = ErrorTypes.None;
        public string UnitPriceOriginalStr { get; set; }
        public BigDecimal? UnitPrice { get; set; }
        public ErrorTypes UnitPriceError { get; set; } = ErrorTypes.None;
        public string PureLoadedCost { get; set; }
        public string CurrencyCode { get; set; }
        public string IntraDiv { get; set; }
        public string DirectIndirect { get; set; }
        public string POTerms { get; set; }
        public string FreightTerms { get; set; }
        public string UOM { get; set; }
        public string TitleTransfer { get; set; }
        public string Port { get; set; }
        public string ReleaseOriginalStr { get; set; }
        public BigDecimal? Release { get; set; }
        public ErrorTypes ReleaseError { get; set; } = ErrorTypes.None;
        public DateTime? CommittedDate { get; set; }
        public string CommittedDateStr { get; set; }
        public string CommittedDateError { get; set; } = "";

        private string _uniquenessKey = "";

        //Ignore this property for entity framework mapping, as it's only used for in-memory duplicate checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UniquenessKey
        {
            get { return _uniquenessKey; }
        }

        public void GenerateUniquenessKey()
        {
            //DIVISION ID|| ~|| LOCAL SITE ID|| ~|| RECEIPT_NUMBER || ~|| PO_NUMBER || ~|| PO_LINE_NUMBER || ~|| PART_NUMBER || ~|| DATE_RECEIVED || ~|| COMMITTED_DATE || ~|| RELEASE#

            //Check for null on dependent fields if null, return empty string
            if (string.IsNullOrWhiteSpace(DivisionId) || string.IsNullOrWhiteSpace(LocalSiteId) || string.IsNullOrWhiteSpace(ReceiptNumber) 
                || string.IsNullOrWhiteSpace(PoNumber) || string.IsNullOrWhiteSpace(POLineNumber) || string.IsNullOrWhiteSpace(PartNumber) 
                || string.IsNullOrWhiteSpace(DateReceivedStr) || string.IsNullOrWhiteSpace(CommittedDateStr) || string.IsNullOrWhiteSpace(ReleaseOriginalStr))
            {
                _uniquenessKey = "";
                return;
            }

            _uniquenessKey = $"{DivisionId.ToLower()}_{LocalSiteId.ToLower()}_{ReceiptNumber.ToLower()}_{PoNumber.ToLower()}_{POLineNumber.ToLower()}_{PartNumber.ToLower()}_{DateReceivedStr.ToLower()}_{CommittedDateStr.ToLower()}_{ReleaseOriginalStr.ToLower()}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }
    }
}
