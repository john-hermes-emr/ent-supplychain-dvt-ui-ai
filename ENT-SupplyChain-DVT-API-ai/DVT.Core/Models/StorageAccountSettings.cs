namespace DVT.Core.Models
{
    public class StorageAccountSettings
    {
        public string MaxFileSizeInBytes { get; set; }
        public string ContainerName { get; set; }
        public string ContainerConnectionString { get; set; }
    }
}
