using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IValidationService
    {
        ValueTask<OperationResult> ValidateFilesAsync(Guid jobId, List<Guid> selectedFileIds, string userEmail);

    }
}
