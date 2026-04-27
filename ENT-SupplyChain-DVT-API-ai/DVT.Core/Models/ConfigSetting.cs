namespace DVT.Core.Models
{
    public class ConfigSetting
    {
        public Guid SettingId { get; set; }
        public string Module { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
    }
}
