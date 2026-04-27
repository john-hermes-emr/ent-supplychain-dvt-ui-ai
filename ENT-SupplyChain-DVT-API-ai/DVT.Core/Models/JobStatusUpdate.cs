namespace DVT.Core.Models
{
    public class JobStatusUpdate
    {
        public JobStatusUpdate(Guid jobId)
        {
            JobId = jobId;
        }

        public JobStatusUpdate(Guid jobId, string jobStatus)
        {
            JobId = jobId;
            JobStatus = jobStatus;
        }

        public void AddFileStatus(Guid fileId, string status)
        {
            JobFileStatus.Add(new KeyValuePair<Guid, string>(fileId, status));
        }

        public Guid JobId { get; set; }
        public List<KeyValuePair<Guid, string>> JobFileStatus { get; set; } = new List<KeyValuePair<Guid, string>>();
        public string JobStatus { get; set; }
    }
}
