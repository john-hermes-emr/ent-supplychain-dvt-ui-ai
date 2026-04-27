namespace DVT.Core.Models
{
    public class FileLoadRequest
    {
        public Guid JobId { get; set; }
        public Guid JobFileId { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public List<List<string>> FileContent { get; set; }
        public string FlatFileContent { get; set; }
    }
}
