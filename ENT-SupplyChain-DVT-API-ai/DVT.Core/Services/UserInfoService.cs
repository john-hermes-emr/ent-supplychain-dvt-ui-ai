using DVT.Core.Models;
using FluentValidation;
using static DVT.Core.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DVT.Core.Services
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UserInfo> _userValidator;
        private readonly IActivityLogService _activityLogService;
        public UserInfoService(IUnitOfWork unitOfWork, IActivityLogService activityLogService, IValidator<UserInfo> userValidator)
        {
            _unitOfWork = unitOfWork;
            _activityLogService = activityLogService;
            _userValidator = userValidator;
        }

        public async ValueTask<UserInfo> GetByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.UserInfos.GetByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException($"{Constants.StardardMessages.ItemNotFound} id: {userId}");

            return user;
        }

        public async ValueTask<UserInfo> GetByEmailAddressAsync(string emailAddress)
        {
            var user = await _unitOfWork.UserInfos.GetByEmailAddressAsync(emailAddress);

            if (user == null)
                throw new KeyNotFoundException($"{Constants.StardardMessages.ItemNotFound} emailAddress: {emailAddress}");

            return user;
        }

        public async ValueTask<UserInfo> SetFoldersAsync(UserInfo user)
        {
            var updateUser = await CheckUserAndJobAsync(user.UserInfoId);

            //if (string.IsNullOrEmpty(user.LoadFolder))
            //    throw new ArgumentNullException(nameof(user.LoadFolder), StardardMessages.LoadDirectoryCannotBeEmpty);

            //if(string.IsNullOrEmpty(user.LogFolder))
            //    throw new ArgumentNullException(nameof(user.LogFolder), StardardMessages.LogDirectoryCannotBeEmpty);

            //if (string.IsNullOrEmpty(user.ProductionFolder))
            //    throw new ArgumentNullException(nameof(user.ProductionFolder), StardardMessages.ProductionDirectoryCannotBeEmpty);

            updateUser.LoadFolder = user.LoadFolder;
            updateUser.LogFolder = user.LogFolder;
            updateUser.ProductionFolder = user.ProductionFolder;

            updateUser.UpdateBy = user.UpdateBy;
            updateUser.UpdateDate = DateTime.UtcNow;

            var validationResult = await _userValidator.ValidateAsync(updateUser);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(StardardMessages.ValidationFailedMsg + errors);
            }

            await _unitOfWork.CommitAsync();

            await AddActivityLog(updateUser.UserInfoId, string.Format(StardardMessages.UserSetFolder, user.UpdateBy, user.LoadFolder, user.LogFolder, user.ProductionFolder), user.UpdateBy);

            return updateUser;
        }

        public async ValueTask<UserInfo> SetLoadFolderAsync(Guid userId, string loadPath, string updateBy)
        {
            var updateUser = await CheckUserAndJobAsync(userId);

            updateUser.LoadFolder = loadPath;
            updateUser.UpdateBy = updateBy;
            updateUser.UpdateDate = DateTime.UtcNow;

            var validationResult = await _userValidator.ValidateAsync(updateUser);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(StardardMessages.ValidationFailedMsg + errors);
            }

            await _unitOfWork.CommitAsync();

            await AddActivityLog(updateUser.UserInfoId, string.Format(StardardMessages.UserSetLoadFolder, updateBy, loadPath), updateBy);

            return updateUser;
        }

        public async ValueTask<UserInfo> SetLogFolderAsync(Guid userId, string logPath, string updateBy)
        {
            var updateUser = await CheckUserAndJobAsync(userId);

            updateUser.LogFolder = logPath;
            updateUser.UpdateBy = updateBy;
            updateUser.UpdateDate = DateTime.UtcNow;

            var validationResult = await _userValidator.ValidateAsync(updateUser);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(StardardMessages.ValidationFailedMsg + errors);
            }

            await _unitOfWork.CommitAsync();

            await AddActivityLog(updateUser.UserInfoId, string.Format(StardardMessages.UserSetLogFolder, updateBy, logPath), updateBy);

            return updateUser;
        }

        public async ValueTask<UserInfo> SetProductionFolderAsync(Guid userId, string prodPath, string updateBy)
        {
            var updateUser = await CheckUserAndJobAsync(userId);

            updateUser.ProductionFolder = prodPath;
            updateUser.UpdateBy = updateBy;
            updateUser.UpdateDate = DateTime.UtcNow;

            var validationResult = await _userValidator.ValidateAsync(updateUser);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(StardardMessages.ValidationFailedMsg + errors);
            }

            await _unitOfWork.CommitAsync();

            await AddActivityLog(updateUser.UserInfoId, string.Format(StardardMessages.UserSetProductionFolder, updateBy, prodPath), updateBy);

            return updateUser;
        }

        private async ValueTask<UserInfo> CheckUserAndJobAsync(Guid userId)
        {
            var updateUser = await _unitOfWork.UserInfos.GetByIdAsync(userId);

            if (updateUser == null)
                throw new KeyNotFoundException($"{Constants.StardardMessages.ItemNotFound} id: {userId}");

            var activeJob = await _unitOfWork.Jobs.GetUserLatestActiveJobAsync(userId);

            if (activeJob != null)
            {
                throw new Exception(StardardMessages.ActiveJobAlreadyExists);
            }

            return updateUser;
        }

        private async Task AddActivityLog(Guid userInfoId, string message, string updateBy)
        {
            await _activityLogService.AddLogAsync(new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = userInfoId,
                Entity = DVTEntities.UserInfo,
                Message = message,
                CreateBy = updateBy,
                CreateDate = DateTime.UtcNow
            });
        }
    }
}
