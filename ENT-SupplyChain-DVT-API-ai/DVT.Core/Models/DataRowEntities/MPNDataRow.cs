namespace DVT.Core.Models.DataRowEntities
{
    public class MPNDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionID { get; set; }
        public string LocalSiteID { get; set; }
        public string PartNumber { get; set; }
        public string LocalManufacturerID { get; set; }
        public string ManufactureID { get; set; }
        public string ManufactureName { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string ObjectID { get; set; }
        public string MPNType { get; set; }

        private string _uniquenessKey = "";

        //Ignore this property for entity framework mapping, as it's only used for in-memory duplicate checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UniquenessKey
        {
            get { return _uniquenessKey; }
        }

        public void GenerateUniquenessKey()
        {
            //|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|MANUFACTURER PART NUMBER|+|LOCAL MANUFACTURER ID|+|MANUFACTURER NAME|

            //Check for null on dependent fields if null, return empty string
            if (string.IsNullOrWhiteSpace(DivisionID) || string.IsNullOrWhiteSpace(LocalSiteID) || string.IsNullOrWhiteSpace(PartNumber)
                || string.IsNullOrWhiteSpace(ManufacturerPartNumber) || string.IsNullOrWhiteSpace(LocalManufacturerID) 
                || string.IsNullOrWhiteSpace(ManufactureName))
            {
                _uniquenessKey = "";
                return;
            }

                _uniquenessKey = $"{DivisionID.ToLower()}_{LocalSiteID.ToLower()}_{PartNumber.ToLower()}_{ManufacturerPartNumber.ToLower()}_{LocalManufacturerID.ToLower()}_{ManufactureName.ToLower()}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }
    }
}
