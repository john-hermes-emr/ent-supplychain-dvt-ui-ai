namespace DVT.Core.Models
{
    public class FileCalculateStatistics_PO
    {
        public int TotalRecords { get; set; }
        public string OrderDateMin { get; set; }
        public string OrderDateMax { get; set; }
        public string LatestAmendmentMin { get; set; }
        public string LatestAmendmentMax { get; set; }
    }
}
