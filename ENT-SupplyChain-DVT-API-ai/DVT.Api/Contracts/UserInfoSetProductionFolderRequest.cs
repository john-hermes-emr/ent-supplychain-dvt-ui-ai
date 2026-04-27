namespace DVT.Api.Contracts
{
    public class UserInfoSetProductionFolderRequest
    {
        public Guid UserInfoId { get; set; }
        public string ProductionFolder { get; set; }
        public string UpdateBy { get; set; }
    }
}
