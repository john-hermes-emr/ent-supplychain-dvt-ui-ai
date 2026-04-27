namespace DVT.Api.Contracts
{
    public class UserInfoSetRequest
    {
        public Guid UserInfoId { get; set; }
        public string LoadFolder { get; set; }
        public string LogFolder { get; set; }
        public string ProductionFolder { get; set; }
        public string UpdateBy { get; set; }
    }
}
