namespace DVT.Api.Contracts.Job
{
    public class JobValidationRequest
    {
        public Guid JobId { get; set; }
        public List<Guid> SelectedFileIds { get; set; }
    }
}
