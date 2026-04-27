using Azure.Storage.Files.Shares;

namespace DVT.Core.Services
{
    public interface IShareClientMainShare
    {
        ShareDirectoryClient GetDirectoryClient(string directory);
    }
}
