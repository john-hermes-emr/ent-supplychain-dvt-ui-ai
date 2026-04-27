namespace DVT.Core.Models
{
    public class UserInfo
    {
        public Guid UserInfoId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string? LoadFolder { get; set; }
        public string? LogFolder { get; set; }
        public string? ProductionFolder { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Deleted { get; set; }
    }
}
