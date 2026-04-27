namespace DVT.Api.Contracts.Job
{
    public class JobCreateRequest
    {
        public Guid DivisionId { get; set; }
        public int FeedNumber { get; set; }
        public Guid UserInfoId { get; set; }
        public bool ForceCreate { get; set; } = false;
    }
}
