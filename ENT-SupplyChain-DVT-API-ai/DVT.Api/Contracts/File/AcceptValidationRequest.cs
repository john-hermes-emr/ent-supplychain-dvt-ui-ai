namespace DVT.Api.Contracts.File
{
    public class AcceptValidationRequest
    {
        public Guid JobId { get; set; }
        public Guid JobFileId { get; set; } 
        public string UpdateBy { get; set; } 
    }
}
