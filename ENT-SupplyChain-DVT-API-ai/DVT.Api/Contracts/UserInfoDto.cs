namespace DVT.Api.Contracts
{
    public class UserInfoDto
    {
        public Guid UserInfoId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string? LoadFolder { get; set; }
        public string? LogFolder { get; set; }
        public string? ProductionFolder { get; set; }

    }
}
