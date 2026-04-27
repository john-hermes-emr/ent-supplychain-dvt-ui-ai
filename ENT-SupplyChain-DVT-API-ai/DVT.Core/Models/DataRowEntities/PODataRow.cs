using DocumentFormat.OpenXml.Wordprocessing;

namespace DVT.Core.Models.DataRowEntities
{
    public class PODataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionID { get; set; }
        public string LocalSiteID { get; set; }
        public string PONumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OrderDateStr { get; set; }
        public string OrderDateError { get; set; } = "";
        public DateTime? LatestAmendment { get; set; }
        public string LatestAmendmentStr { get; set; }
        public string LatestAmendmentError { get; set; } = "";
        public string CommodityMGRId { get; set; }
        public string SupplierID { get; set; }
        public string CurrencyCode { get; set; }
        public string POType { get; set; }
        public string IntraDiv { get; set; }
        public string DirectIndirect { get; set; }
        public string POTerms { get; set; }
        public string FreightTerms { get; set; }
        public string EDI { get; set; }
        public string OrderStatus { get; set; }
        public string TitleTransfer { get; set; }
        public string Port { get; set; }

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
            if (string.IsNullOrWhiteSpace(DivisionID) || string.IsNullOrWhiteSpace(LocalSiteID) || string.IsNullOrWhiteSpace(PONumber))
            {
                _uniquenessKey = "";
                return;
            }

            _uniquenessKey = $"{DivisionID.ToLower()}_{LocalSiteID.ToLower()}_{PONumber.ToLower()}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }
    }
}
