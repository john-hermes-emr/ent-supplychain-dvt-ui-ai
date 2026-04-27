using Azure.Storage.Files.Shares;

namespace DVT.Core.Services
{
    public interface IShareClientUserShare
    {
        ShareDirectoryClient GetDirectoryClient(string directory);
    }
}
