using DocumentFormat.OpenXml.Wordprocessing;

namespace DVT.Core.Models.DataRowEntities
{
    public class SupplierDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionId { get; set; }
        public string LocalSiteId { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string DUNS { get; set; }
        public string ActiveInactive { get; set; }
        public string DirectIndirect { get; set; }
        public string AddressDescr { get; set; }
        public string Street { get; set; }
        public string Suite { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Addr3 { get; set; }
        public string Addr4 { get; set; }
        public string CountryCode { get; set; }
        public string GlobalFlag { get; set; }
        public string MainTelephone { get; set; }
        public string TollFree { get; set; }
        public string Fax { get; set; }
        public string WebSite { get; set; }
        public string SupplierType { get; set; }

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
            if (string.IsNullOrWhiteSpace(DivisionId) || string.IsNullOrWhiteSpace(LocalSiteId) || string.IsNullOrWhiteSpace(SupplierId))
            {
                _uniquenessKey = "";
                return;
            }

            _uniquenessKey = $"{DivisionId.ToLower()}_{LocalSiteId.ToLower()}_{SupplierId.ToLower()}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }

    }
}
