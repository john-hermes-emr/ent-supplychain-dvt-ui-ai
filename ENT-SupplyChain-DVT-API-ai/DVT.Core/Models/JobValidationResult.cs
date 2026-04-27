namespace DVT.Core.Models
{
    public class JobValidationResult
    {
        public bool Success { get; set; }

        public string  ExceptionMessage { get; set; }

        public List<FileValidationResult> FileValidationErrors { get; set; } = new List<FileValidationResult>();

    }
}
