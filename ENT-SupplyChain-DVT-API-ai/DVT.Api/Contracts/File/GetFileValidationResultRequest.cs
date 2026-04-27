namespace DVT.Api.Contracts.File
{
    public class GetFileValidationResultRequest
    {
        public Guid JobId { get; set; }
        public Guid JobFileId { get; set; }
    }
}
