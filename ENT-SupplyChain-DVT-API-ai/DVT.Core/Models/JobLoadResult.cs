namespace DVT.Core.Models
{
    public class JobLoadResult
    {
        public string Operation { get; set; }
        public bool Success
        {
            get
            {
                return FileLoadResults.All(f => f.Success);
            }
        }
        public string Message
        {
            get
            {
                var message = string.Join(Environment.NewLine, FileLoadResults.Select(f => f.Message));

                return string.IsNullOrEmpty(message) ? "All files loaded successfully." : message;
            }
        }
        public List<FileLoadResult> FileLoadResults { get; set; } = new List<FileLoadResult>();

        public FileLoadResult GetFileLoadResultByJobFileId(Guid jobFileId)
        {
            return FileLoadResults.FirstOrDefault(f => f.JobFileId == jobFileId);
        }
    }
}
