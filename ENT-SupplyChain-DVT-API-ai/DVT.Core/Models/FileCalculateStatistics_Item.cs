namespace DVT.Core.Models
{
    public class FileCalculateStatistics_Item : IFileCalculateStatistics
    {
        public int TotalRecords { get; set; }
        public string StandardCostMin { get; set; }
        public string StandardCostMax { get; set; }
    }
}
