namespace DVT.Core.Models
{
    internal class FileCalculateStatistics_UOM : IFileCalculateStatistics
    {
        public int TotalRecords { get; set; }
        public string ConversionRateMin { get; set; }
        public string ConversionRateMax { get; set; }
    }
}
