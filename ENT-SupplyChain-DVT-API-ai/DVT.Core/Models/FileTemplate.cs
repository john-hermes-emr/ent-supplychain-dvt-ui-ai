namespace DVT.Core.Models
{
    public class FileTemplate
    {
        public string Table { get; set; }
        public string FileType { get; set; }
        public string FileNameFormat { get; set; }
        public int SortOrder { get; set; }
        public string DependsOnFileTypes { get; set; }

    }
}
