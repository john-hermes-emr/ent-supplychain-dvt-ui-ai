namespace DVT.Core.Models
{
    public class FileCalculateStatistics_Vir : IFileCalculateStatistics
    {
        public int TotalRecords { get; set; }
        public string QuantityOrderedMin { get; set; }
        public string QuantityOrderedMax { get; set; }
        public string QuantityReceivedMin { get; set; }
        public string QuantityReceivedMax { get; set; }
        public string DateReceivedMin { get; set; }
        public string DateReceivedMax { get; set; }
        public string InvoicePricePaidMin { get; set; }
        public string InvoicePricePaidMax { get; set; }
        public string UnitPriceMin { get; set; }
        public string UnitPriceMax { get; set; }
        public string CommittedDateMin { get; set; }
        public string CommittedDateMax { get; set; }
    }
}
