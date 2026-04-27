namespace DVT.Core.Models
{
    public class Job
    {
        public Job()
        {
            JobFiles = new List<JobFile>();
        }

        public Guid JobId { get; set; }
        public Guid DivisionId { get; set; }
        public string Status { get; set; }
        public int FeedNumber { get; set; }
        public string? ArchiveFilePath { get; set; }
        public Guid UserInfoId { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool Deleted { get; set; }

        public virtual List<JobFile> JobFiles { get; set; }
    }
}
