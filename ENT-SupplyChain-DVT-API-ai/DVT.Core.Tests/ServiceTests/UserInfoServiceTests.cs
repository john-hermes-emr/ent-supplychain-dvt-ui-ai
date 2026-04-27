using DVT.Core;
using DVT.Core.Models;
using DVT.Core.Services;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ServiceTests
{
    public class UserInfoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IValidator<UserInfo>> _validatorMock;
        private readonly UserInfoService _service;

        public UserInfoServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _validatorMock = new Mock<IValidator<UserInfo>>();
            _service = new UserInfoService(
                _unitOfWorkMock.Object,
                _activityLogServiceMock.Object,
                _validatorMock.Object
            );
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUserInfo_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userInfo = new UserInfo
            {
                UserInfoId = userId,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@test.com",
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(userInfo);

            // Act
            var result = await _service.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserInfoId);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john.doe@test.com", result.EmailAddress);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync((UserInfo)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetByIdAsync(userId));
            Assert.Contains("Item was not found", exception.Message);
            Assert.Contains(userId.ToString(), exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldVerifyRepositoryCalledOnce()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userInfo = new UserInfo { UserInfoId = userId, EmailAddress = "test@test.com" };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(userInfo);

            // Act
            await _service.GetByIdAsync(userId);

            // Assert
            _unitOfWorkMock.Verify(x => x.UserInfos.GetByIdAsync(userId), Times.Once);
        }

        #endregion

        #region GetByEmailAddressAsync Tests

        [Fact]
        public async Task GetByEmailAddressAsync_ShouldReturnUserInfo_WhenEmailExists()
        {
            // Arrange
            var emailAddress = "test@domain.com";
            var userInfo = new UserInfo
            {
                UserInfoId = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                EmailAddress = emailAddress,
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByEmailAddressAsync(emailAddress))
                .ReturnsAsync(userInfo);

            // Act
            var result = await _service.GetByEmailAddressAsync(emailAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(emailAddress, result.EmailAddress);
            Assert.Equal("Jane", result.FirstName);
        }

        [Fact]
        public async Task GetByEmailAddressAsync_ShouldThrowException_WhenEmailDoesNotExist()
        {
            // Arrange
            var emailAddress = "nonexistent@domain.com";
            _unitOfWorkMock.Setup(x => x.UserInfos.GetByEmailAddressAsync(emailAddress))
                .ReturnsAsync((UserInfo)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetByEmailAddressAsync(emailAddress));
            Assert.Contains("Item was not found", exception.Message);
            Assert.Contains(emailAddress, exception.Message);
        }

        [Fact]
        public async Task GetByEmailAddressAsync_ShouldBeCaseInsensitive()
        {
            // Arrange
            var emailAddress = "Test@Domain.COM";
            var userInfo = new UserInfo
            {
                UserInfoId = Guid.NewGuid(),
                EmailAddress = emailAddress
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByEmailAddressAsync(emailAddress))
                .ReturnsAsync(userInfo);

            // Act
            var result = await _service.GetByEmailAddressAsync(emailAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(emailAddress, result.EmailAddress);
        }

        #endregion

        #region SetFoldersAsync Tests

        [Fact]
        public async Task SetFoldersAsync_ShouldUpdateFolders_WhenUserExistsAndNoActiveJob()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo
            {
                UserInfoId = userId,
                EmailAddress = "user@test.com",
                LoadFolder = "oldLoad",
                LogFolder = "oldLog",
                ProductionFolder = "oldProd"
            };
            var updatedUser = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "newLoad",
                LogFolder = "newLog",
                ProductionFolder = "newProd",
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetFoldersAsync(updatedUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newLoad", result.LoadFolder);
            Assert.Equal("newLog", result.LogFolder);
            Assert.Equal("newProd", result.ProductionFolder);
            Assert.Equal("admin", result.UpdateBy);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SetFoldersAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod",
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync((UserInfo)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.SetFoldersAsync(user));
        }

        [Fact]
        public async Task SetFoldersAsync_ShouldThrowException_WhenActiveJobExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var activeJob = new Job { JobId = Guid.NewGuid(), UserInfoId = userId };
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod",
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync(activeJob);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.SetFoldersAsync(user));
            Assert.Equal(StardardMessages.ActiveJobAlreadyExists, exception.Message);
        }

        [Fact]
        public async Task SetFoldersAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod",
                UpdateBy = "admin"
            };
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("LoadFolder", "Invalid folder path")
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await _service.SetFoldersAsync(user));
        }

        [Fact]
        public async Task SetFoldersAsync_ShouldLogActivity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod",
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetFoldersAsync(user);

            // Assert
            _activityLogServiceMock.Verify(
                x => x.AddLogAsync(It.Is<ActivityLog>(
                    log => log.EntityId == userId &&
                    log.Entity == DVTEntities.UserInfo &&
                    log.CreateBy == "admin")),
                Times.Once);
        }

        #endregion

        #region SetLoadFolderAsync Tests

        [Fact]
        public async Task SetLoadFolderAsync_ShouldUpdateLoadFolder_WhenUserExistsAndNoActiveJob()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var loadPath = "newLoadFolder";
            var updateBy = "admin";
            var existingUser = new UserInfo
            {
                UserInfoId = userId,
                EmailAddress = "user@test.com",
                LoadFolder = "oldLoad"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetLoadFolderAsync(userId, loadPath, updateBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loadPath, result.LoadFolder);
            Assert.Equal(updateBy, result.UpdateBy);
        }

        [Fact]
        public async Task SetLoadFolderAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync((UserInfo)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.SetLoadFolderAsync(userId, "load", "admin"));
        }

        [Fact]
        public async Task SetLoadFolderAsync_ShouldThrowException_WhenActiveJobExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var activeJob = new Job { JobId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync(activeJob);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.SetLoadFolderAsync(userId, "load", "admin"));
            Assert.Equal(StardardMessages.ActiveJobAlreadyExists, exception.Message);
        }

        [Fact]
        public async Task SetLoadFolderAsync_ShouldLogActivityWithCorrectMessage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var loadPath = "testLoadFolder";
            var updateBy = "testUser";
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetLoadFolderAsync(userId, loadPath, updateBy);

            // Assert
            _activityLogServiceMock.Verify(
                x => x.AddLogAsync(It.Is<ActivityLog>(
                    log => log.Message.Contains(updateBy) &&
                    log.Message.Contains(loadPath))),
                Times.Once);
        }

        #endregion

        #region SetLogFolderAsync Tests

        [Fact]
        public async Task SetLogFolderAsync_ShouldUpdateLogFolder_WhenUserExistsAndNoActiveJob()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var logPath = "newLogFolder";
            var updateBy = "admin";
            var existingUser = new UserInfo
            {
                UserInfoId = userId,
                EmailAddress = "user@test.com",
                LogFolder = "oldLog"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetLogFolderAsync(userId, logPath, updateBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(logPath, result.LogFolder);
            Assert.Equal(updateBy, result.UpdateBy);
        }

        [Fact]
        public async Task SetLogFolderAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync((UserInfo)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.SetLogFolderAsync(userId, "log", "admin"));
        }

        [Fact]
        public async Task SetLogFolderAsync_ShouldThrowException_WhenActiveJobExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var activeJob = new Job { JobId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync(activeJob);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.SetLogFolderAsync(userId, "log", "admin"));
            Assert.Equal(StardardMessages.ActiveJobAlreadyExists, exception.Message);
        }

        [Fact]
        public async Task SetLogFolderAsync_ShouldUpdateDateTimeToUtc()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var beforeUpdate = DateTime.UtcNow;

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetLogFolderAsync(userId, "log", "admin");
            var afterUpdate = DateTime.UtcNow;

            // Assert
            Assert.True(result.UpdateDate >= beforeUpdate);
            Assert.True(result.UpdateDate <= afterUpdate);
        }

        #endregion

        #region SetProductionFolderAsync Tests

        [Fact]
        public async Task SetProductionFolderAsync_ShouldUpdateProductionFolder_WhenUserExistsAndNoActiveJob()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var prodPath = "newProdFolder";
            var updateBy = "admin";
            var existingUser = new UserInfo
            {
                UserInfoId = userId,
                EmailAddress = "user@test.com",
                ProductionFolder = "oldProd"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetProductionFolderAsync(userId, prodPath, updateBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(prodPath, result.ProductionFolder);
            Assert.Equal(updateBy, result.UpdateBy);
        }

        [Fact]
        public async Task SetProductionFolderAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync((UserInfo)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.SetProductionFolderAsync(userId, "prod", "admin"));
        }

        [Fact]
        public async Task SetProductionFolderAsync_ShouldThrowException_WhenActiveJobExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var activeJob = new Job { JobId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync(activeJob);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.SetProductionFolderAsync(userId, "prod", "admin"));
            Assert.Equal(StardardMessages.ActiveJobAlreadyExists, exception.Message);
        }

        [Fact]
        public async Task SetProductionFolderAsync_ShouldCommitChanges()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetProductionFolderAsync(userId, "prod", "admin");

            // Assert
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task SetFoldersAsync_ShouldInvokeValidator()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod",
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetFoldersAsync(user);

            // Assert
            _validatorMock.Verify(
                x => x.ValidateAsync(It.IsAny<UserInfo>(), default),
                Times.Once);
        }

        [Theory]
        [InlineData("load")]
        [InlineData("log")]
        [InlineData("prod")]
        public async Task SetLoadFolderAsync_ShouldValidateWithDifferentPaths(string path)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetLoadFolderAsync(userId, path, "admin");

            // Assert
            Assert.Equal(path, result.LoadFolder);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task SetFoldersAsync_ShouldUpdateAllThreeFolders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo
            {
                UserInfoId = userId,
                EmailAddress = "user@test.com",
                LoadFolder = "oldLoad",
                LogFolder = "oldLog",
                ProductionFolder = "oldProd"
            };
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = "newLoad",
                LogFolder = "newLog",
                ProductionFolder = "newProd",
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetFoldersAsync(user);

            // Assert
            Assert.Equal("newLoad", result.LoadFolder);
            Assert.Equal("newLog", result.LogFolder);
            Assert.Equal("newProd", result.ProductionFolder);
        }

        [Fact]
        public async Task MultipleSetFolderOperations_ShouldWorkSequentially()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo
            {
                UserInfoId = userId,
                EmailAddress = "user@test.com"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result1 = await _service.SetLoadFolderAsync(userId, "load1", "admin");
            var result2 = await _service.SetLogFolderAsync(userId, "log1", "admin");
            var result3 = await _service.SetProductionFolderAsync(userId, "prod1", "admin");

            // Assert
            Assert.Equal("load1", result1.LoadFolder);
            Assert.Equal("log1", result2.LogFolder);
            Assert.Equal("prod1", result3.ProductionFolder);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Exactly(3));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task SetFoldersAsync_ShouldHandleNullFolderPaths()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };
            var user = new UserInfo
            {
                UserInfoId = userId,
                LoadFolder = null,
                LogFolder = null,
                ProductionFolder = null,
                UpdateBy = "admin"
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetFoldersAsync(user);

            // Assert
            Assert.Null(result.LoadFolder);
            Assert.Null(result.LogFolder);
            Assert.Null(result.ProductionFolder);
        }

        [Fact]
        public async Task SetLoadFolderAsync_ShouldHandleEmptyString()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new UserInfo { UserInfoId = userId, EmailAddress = "user@test.com" };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(x => x.Jobs.GetUserLatestActiveJobAsync(userId))
                .ReturnsAsync((Job)null);
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UserInfo>(), default))
                .ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetLoadFolderAsync(userId, string.Empty, "admin");

            // Assert
            Assert.Equal(string.Empty, result.LoadFolder);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUserWithAllProperties()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userInfo = new UserInfo
            {
                UserInfoId = userId,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@test.com",
                LoadFolder = "load",
                LogFolder = "log",
                ProductionFolder = "prod",
                UpdateBy = "system",
                UpdateDate = DateTime.UtcNow,
                Deleted = false
            };

            _unitOfWorkMock.Setup(x => x.UserInfos.GetByIdAsync(userId))
                .ReturnsAsync(userInfo);

            // Act
            var result = await _service.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserInfoId);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john.doe@test.com", result.EmailAddress);
            Assert.Equal("load", result.LoadFolder);
            Assert.Equal("log", result.LogFolder);
            Assert.Equal("prod", result.ProductionFolder);
            Assert.Equal("system", result.UpdateBy);
            Assert.False(result.Deleted);
        }

        #endregion
    }
}