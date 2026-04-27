namespace DVT.Core.Models
{
    public class SiteMaster
    {
        public string DivisionId { get; set; }
        public string LocalSiteId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime LastUpdateDate { get; set; }
        public string LastUpdateBy { get; set; }
    }
}
