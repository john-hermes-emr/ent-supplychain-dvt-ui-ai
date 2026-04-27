namespace DVT.Api.Contracts
{
    public class OptionListItemDto
    {
        public Guid OptionId { get; set; }
        public string OptionName { get; set; }
        public string CategoryName { get; set; }
        public string? HelpText { get; set; }
        public int? SortField { get; set; } = null!;
    }
}
