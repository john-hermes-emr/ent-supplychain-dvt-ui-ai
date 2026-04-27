using DVT.Api.Contracts.File;

namespace DVT.Api.Contracts.Job
{
    public class JobLoadRequest
    {
        public Guid DivisionId { get; set; }
        public int FeedNumber { get; set; }
        public Guid UserId { get; set; }
        public List<FileLoadRequest> FileList { get; set; }
    }
}
