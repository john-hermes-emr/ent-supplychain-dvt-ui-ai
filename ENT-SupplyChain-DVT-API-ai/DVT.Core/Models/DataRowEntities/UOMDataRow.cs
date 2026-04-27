using DVT.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DVT.Core.Constants;

namespace DVT.Core.Models.DataRowEntities
{
    public class UOMDataRow : IDataRow
    {
        public int RowNumber { get; set; }
        public string DivisionID { get; set; }
        public string LocalSiteID { get; set; }
        public string PartNumber { get; set; }
        public string LocalUOM { get; set; }
        public string BaseUOM { get; set; }
        public string ConversionRateOriginalStr { get; set; }
        public BigDecimal? ConversionRate { get; set; }
        public ErrorTypes ConversionRateError { get; set; } = ErrorTypes.None;

        private string _uniquenessKey = "";

        //Ignore this property for entity framework mapping, as it's only used for in-memory duplicate checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UniquenessKey
        {
            get { return _uniquenessKey; }
        }

        public void GenerateUniquenessKey()
        {
            //|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|LOCAL UOM|+|BASE UOM|
            //Check for null on dependent fields if null, return empty string
            if (string.IsNullOrWhiteSpace(DivisionID) || string.IsNullOrWhiteSpace(LocalSiteID) || string.IsNullOrWhiteSpace(PartNumber)
                || string.IsNullOrWhiteSpace(LocalUOM) || string.IsNullOrWhiteSpace(BaseUOM))
            {
                _uniquenessKey = "";
                return;
            }
            _uniquenessKey = $"{DivisionID}_{LocalSiteID}_{PartNumber}_{LocalUOM}_{BaseUOM}";
        }

        //Ignore this property for entity framework mapping, as it's only used for in-memory incorrect column count checking
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IncorrectColumnCount { get; set; }
    }
}
