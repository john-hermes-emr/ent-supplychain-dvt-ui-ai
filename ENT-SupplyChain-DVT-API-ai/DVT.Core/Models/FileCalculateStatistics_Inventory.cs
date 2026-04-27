namespace DVT.Core.Models
{
    public class FileCalculateStatistics_Inventory : IFileCalculateStatistics
    {
        public int TotalRecords { get; set; }
        public string QuantityMin { get; set; }
        public string QuantityMax { get; set; }
        public string StandardCostMin { get; set; }
        public string StandardCostMax { get; set; }
        public string TotalValueMin { get; set; }
        public string TotalValueMax { get; set; }
        public string InventoryDateMin { get; set; }
        public string InventoryDateMax { get; set; }
    }
}
