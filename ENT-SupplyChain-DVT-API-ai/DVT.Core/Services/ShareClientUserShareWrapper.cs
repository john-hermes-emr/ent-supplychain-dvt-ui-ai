using Azure.Storage.Files.Shares;

namespace DVT.Core.Services
{
    public class ShareClientUserShareWrapper : ShareClient, IShareClientUserShare
    {
        public ShareClientUserShareWrapper(string connectionString, string shareName)
            : base(connectionString, shareName) { }

        public ShareDirectoryClient GetDirectoryClient(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("Directory cannot be null or empty.", nameof(directory));
            }
            return base.GetDirectoryClient(directory);
        }
    }
}