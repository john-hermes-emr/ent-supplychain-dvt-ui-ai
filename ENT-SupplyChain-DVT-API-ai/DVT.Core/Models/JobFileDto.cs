using System.ComponentModel.DataAnnotations.Schema;

namespace DVT.Core.Models
{
    public class JobFileDto
    {
        public Guid JobFileId { get; set; }
        public Guid JobId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string TableName { get; set; }
        public string FileType { get; set; }
        public int SortOrder { get; set; }
        public string DependsOnFileType { get; set; }
        public string Status { get; set; }
        public DateTime? FileCreationTimestamp { get; set; }
        public DateTime? FileLastModifiedTimestamp { get; set; }
        public int RecordCount { get; set; }
        public DateTime? LoadDate { get; set; }
        [Column(TypeName = "json")]
        public string? ValidationMessages { get; set; }
        [Column(TypeName = "json")]
        public string? ValidationStats { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
