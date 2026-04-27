namespace DVT.Core.Models
{
    public class FileValidationResult
    {
        public Guid JobFileId { get; set; }
        public string FileName { get; set; }
        public List<FileRowValidationResult> RowValidationResults { get; set; } = new List<FileRowValidationResult>();

        public string AdditionalInfo { get; set; }

        public bool IsValid
        {
            get
            {
                //If there are any row validation results that are not valid, then the whole file is not valid.
                return !RowValidationResults.Any(v => !v.IsValid);
            }
        }

        public FileValidationResult()
        {

        }
        public FileValidationResult(Guid jobFileId, string fileName)
        {
            JobFileId = jobFileId;
            FileName = fileName;
        }

        public static FileValidationResult Combine(FileValidationResult result1, FileValidationResult result2)
        {
            var combinedResult = new FileValidationResult
            {
                JobFileId = result1.JobFileId,
                FileName = result1.FileName,
                RowValidationResults = result1.RowValidationResults.Concat(result2.RowValidationResults).ToList()
            };
            return combinedResult;
        }
    }
}
