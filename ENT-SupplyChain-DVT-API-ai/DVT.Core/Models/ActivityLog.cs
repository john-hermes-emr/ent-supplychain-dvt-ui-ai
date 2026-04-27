using static DVT.Core.Constants;

namespace DVT.Core.Models
{
    public class ActivityLog
    {
        public Guid LogId { get; set; }
        public Guid EntityId { get; set; }
        public string Entity { get; set; }
        public string MessageType { get; set; } = LogMessageTypes.Info;
        public string Message { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Deleted { get; set; }
    }
}
