namespace DVT.Core.Models
{
    public interface IDataRow
    {
        public int RowNumber { get; set; }
        public bool IncorrectColumnCount { get; set; }
        public string UniquenessKey { get; }
        public void GenerateUniquenessKey();
    }
}
