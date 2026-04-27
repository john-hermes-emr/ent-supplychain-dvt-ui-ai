namespace DVT.Core.Models
{
    public class JobCreation
    {
        public Guid UserId { get; set; }
        public Guid DivisionId { get; set; }
        public int FeedNumber { get; set; }

        public JobCreation(Guid userId, Guid divisionId, int feedNumber)
        {
            UserId = userId;
            DivisionId = divisionId;
            FeedNumber = feedNumber;
        }
    }
}
