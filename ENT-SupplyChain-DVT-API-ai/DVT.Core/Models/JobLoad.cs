namespace DVT.Core.Models
{
    public class JobLoad
    {
        public Guid JobId { get; set; }
        public Guid DivisionId { get; set; }
        public int FeedNumber { get; set; }
        public string UserEmail { get; set; }
        public List<FileLoadRequest> FileList { get; set; }
    }
}
