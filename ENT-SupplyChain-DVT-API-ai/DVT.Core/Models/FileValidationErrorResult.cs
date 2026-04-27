namespace DVT.Core.Models
{
    public class FileValidationErrorResult
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string TableName { get; set; }
        public DateTime Date { get; set; }      
        public List<FileValidationSummarized> Summarizeds { get; set; }
        //public List<FileRowValidationResult> Errors { get; set; }
    }
}
