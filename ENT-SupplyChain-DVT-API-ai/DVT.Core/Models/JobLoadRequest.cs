namespace DVT.Core.Models
{
    public class JobLoadRequest
    {
        public Guid JobId { get; set; }
        public List<FileLoadRequest> FileList { get; set; }
    }
}
