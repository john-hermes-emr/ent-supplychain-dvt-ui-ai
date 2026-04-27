namespace DVT.Api.Contracts
{
    public class UserInfoSetLogFolderRequest
    {
        public Guid UserInfoId { get; set; }
        public string LogFolder { get; set; }
        public string UpdateBy { get; set; }
    }
}
