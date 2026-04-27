namespace DVT.Core.Models
{
    public class FileLoad
    {
        public Guid JobFileId { get; set; }

        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool Selected { get; set; }
        public List<List<string>> FileContent { get; set; }
    }
}
