namespace DVT.Core.Models
{
    public class FileValidationSummarized
    {
        public Guid GroupId { get; set; }
        public string MessageType { get; set; }
        public string Field { get; set; }
        public int Count { get; set; }
        public string Error { get; set; }
        public List<FileRowValidationSummarizedDetail> Details { get; set; } = new List<FileRowValidationSummarizedDetail>();
    }
}
