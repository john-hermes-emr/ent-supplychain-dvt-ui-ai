namespace DVT.Core.Models
{
    public class FileInfo
    {
        public string FileType { get; set; }
        public string FileNameFormat { get; set; }
        public string DependsOnFileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileUri { get; set; }
        public string FileContent { get; set; }
        public int RecordCount { get; set; }
        public DateTime? LoadDate { get; set; }
        public DateTime? FileCreationTimestamp { get; set; }
        public DateTime? FileLastModifiedTimestamp { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
