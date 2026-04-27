namespace DVT.Api.Contracts.Job
{
    public class CompleteJobRequest
    {
        public Guid JobId { get; set; }
        public List<Guid> SelectedFileIds { get; set; }
    }
}
