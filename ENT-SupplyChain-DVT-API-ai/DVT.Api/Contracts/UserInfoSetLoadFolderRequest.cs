namespace DVT.Api.Contracts
{
    public class UserInfoSetLoadFolderRequest
    {
        public Guid UserInfoId { get; set; }
        public string LoadFolder { get; set; }
        public string UpdateBy { get; set; }
    }
}
