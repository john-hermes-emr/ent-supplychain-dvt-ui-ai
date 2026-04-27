using DVT.Core.Models;
using FluentValidation;

namespace DVT.Core.Services
{
    public class ActivityLogService: IActivityLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ActivityLog> _validator;

        public ActivityLogService(IUnitOfWork unitOfWork, IValidator<ActivityLog> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async ValueTask<IEnumerable<ActivityLog>> GetByEntityIdAsync(Guid id)
        {
            var logs = await _unitOfWork.ActivityLogs.GetByEntityId(id);

            if (logs == null)
                throw new KeyNotFoundException($"{Constants.StardardMessages.ItemNotFound} id: {id}");

            return logs;
        }

        public async Task AddLogAsync(ActivityLog log)
        {
            var validationResult = await _validator.ValidateAsync(log);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _unitOfWork.ActivityLogs.AddAsync(log);
            await _unitOfWork.CommitAsync();
        }

        public async Task AddLogsAsync(List<ActivityLog> logs)
        {
           foreach (var log in logs)
           {
                var validationResult = await _validator.ValidateAsync(log);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }
            await _unitOfWork.ActivityLogs.AddRangeAsync(logs);
            await _unitOfWork.CommitAsync();
        }
    }
}
