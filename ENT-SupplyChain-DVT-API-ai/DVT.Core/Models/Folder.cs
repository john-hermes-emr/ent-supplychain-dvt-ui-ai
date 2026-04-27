namespace DVT.Core.Models
{
    public class Folder
    {
        public string Name { get; set; }
        public List<Folder> Children { get; set; }
    }
}
