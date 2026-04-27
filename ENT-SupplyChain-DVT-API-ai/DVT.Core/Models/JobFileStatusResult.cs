namespace DVT.Core.Models
{
    public class JobFileStatusResult
    {
        public Guid JobFileId { get; set; }
        public Guid JobId { get; set; }
        public string Status { get; set; }
    }
}
