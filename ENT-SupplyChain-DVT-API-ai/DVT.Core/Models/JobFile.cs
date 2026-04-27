using System.ComponentModel.DataAnnotations.Schema;

namespace DVT.Core.Models
{
    public class JobFile
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
        //Below 2 timestamps are from the source file in user load folder
        public DateTime? FileCreationTimestamp { get; set; }
        public DateTime? FileLastModifiedTimestamp { get; set; }
        public int RecordCount { get; set; }
        //Will change after load, validation
        public DateTime? LoadDate { get; set; }
        [Column(TypeName = "json")]
        public string? ValidationMessages { get; set; }
        [Column(TypeName = "json")]
        public string? ValidationStats { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Deleted { get; set; }

        public string GetValidationMessageFileName()
        {
            if (JobFileId == Guid.Empty || string.IsNullOrEmpty(FileType))
                return "ERROR: Missing JobFileId or FileType";

            return $"val_results_{JobFileId.ToString().Substring(0, 8).ToUpper()}_{FileType}.json";
        }
    }
}
