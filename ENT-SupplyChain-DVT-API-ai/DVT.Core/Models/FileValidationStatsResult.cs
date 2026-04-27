namespace DVT.Core.Models
{
    public class FileValidationStatsResult
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string TableName { get; set; }
        public DateTime Date { get; set; }
        public object Stats { get; set; }
    }
}
