namespace DVT.Api.Contracts
{
    public class MasterDataDto
    {
        public Guid ItemId { get; set; }
        public string TableName { get; set; }
        public string TextId { get; set; }
        public string ItemName { get; set; }
        public string ItemNameAbbrev { get; set; }
        public string? Text1 { get; set; }
        public string? Text2 { get; set; }
        public string? Text3 { get; set; }
        public string? Text4 { get; set; }
        public string? Text5 { get; set; }
        public string? Text6 { get; set; }
    }
}
