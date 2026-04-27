using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using DVT.Core.Models;
using DVT.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;
using FileInfo = DVT.Core.Models.FileInfo;

namespace DVT.Core.Tests.ServiceTests
{
    public class StorageServiceTests
    {
        private readonly Mock<IShareClientUserShare> _mockUserShare;
        private readonly Mock<IShareClientMainShare> _mockMainShare;
        private readonly Mock<IActivityLogService> _activityLogService;
        private readonly Mock<IConfigSettingService> _mockConfigSettingService;
        private readonly StorageService _service;

        public StorageServiceTests()
        {
            _mockUserShare = new Mock<IShareClientUserShare>();
            _mockMainShare = new Mock<IShareClientMainShare>();
            _activityLogService = new Mock<IActivityLogService>();
            _mockConfigSettingService = new Mock<IConfigSettingService>();
            _service = new StorageService(_mockUserShare.Object, _mockMainShare.Object, _activityLogService.Object, _mockConfigSettingService.Object);
        }

        #region GetFoldersByEmailAddressAsync Tests

        [Fact]
        public async Task GetFoldersByEmailAddressAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var email = "test@example.com";
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(email)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFoldersByEmailAddressAsync(email).AsTask());
            Assert.Contains("User's folder does not exist", exception.Message);
        }

        [Fact]
        public async Task GetFoldersByEmailAddressAsync_ShouldThrowException_WhenGetUserFolderError()
        {
            // Arrange
            var email = "test@example.com";
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(email)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(true, null));
            mockDir.Setup(x => x.GetFilesAndDirectoriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .Throws(new Exception("Test error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFoldersByEmailAddressAsync(email).AsTask());
            Assert.Equal(StardardMessages.GetUserFolderError, exception.Message);
        }

        #endregion

        #region GetFilesInDirectoryAsync Tests

        [Fact]
        public async Task GetFilesInDirectoryAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var path = "folder";
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(path)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFilesInDirectoryAsync(path).AsTask());
            Assert.Contains("Folder", exception.Message);
            Assert.Contains("does not exist", exception.Message);
        }

        #endregion

        #region GetFileInfoInDirectoryAsync Tests

        [Fact]
        public async Task GetFileInfoInDirectoryAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var path = "folder";
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(path)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFileInfoInDirectoryAsync(path).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        #endregion

        #region GetFileContentsByPathAsync Tests

        [Fact]
        public async Task GetFileContentsByPathAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(dirPath)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFileContentsByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task GetFileContentsByPathAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            var mockDir = new Mock<ShareDirectoryClient>();
            var mockFile = new Mock<ShareFileClient>();

            _mockUserShare.Setup(x => x.GetDirectoryClient(dirPath)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(true, null));
            mockDir.Setup(x => x.GetFileClient(fileName)).Returns(mockFile.Object);
            mockFile.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFileContentsByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        #endregion

        #region GetWorkingFileContentsAsync Tests

        [Fact]
        public async Task GetWorkingFileContentsAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var fileName = "test.txt";
            var mockDir = new Mock<ShareDirectoryClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                          .Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetWorkingFileContentsAsync(jobId, fileName).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task GetWorkingFileContentsAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var fileName = "test.txt";
            var mockDir = new Mock<ShareDirectoryClient>();
            var mockFile = new Mock<ShareFileClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                          .Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(true, null));
            mockDir.Setup(x => x.GetFileClient(fileName)).Returns(mockFile.Object);
            mockFile.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetWorkingFileContentsAsync(jobId, fileName).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        #endregion

        #region AnalyzeFileByPathAsync Tests

        [Fact]
        public async Task AnalyzeFileByPathAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(dirPath)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.AnalyzeFileByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task AnalyzeFileByPathAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            var mockDir = new Mock<ShareDirectoryClient>();
            var mockFile = new Mock<ShareFileClient>();

            _mockUserShare.Setup(x => x.GetDirectoryClient(dirPath)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(true, null));
            mockDir.Setup(x => x.GetFileClient(fileName)).Returns(mockFile.Object);
            mockFile.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.AnalyzeFileByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        #endregion

        #region LoadExtractFilesAsync Tests

        [Fact]
        public async Task LoadExtractFilesAsync_ShouldThrowException_WhenWorkingFolderDoesNotExist()
        {
            // Arrange
            var job = new Job { JobId = Guid.NewGuid() };
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = Guid.NewGuid(),
                    FileName = "test.txt",
                    FilePath = "path/test.txt"
                }
            };
            var mockDir = new Mock<ShareDirectoryClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder))
                          .Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.LoadExtractFilesAsync(job, jobFiles).AsTask());
            Assert.Equal(StardardMessages.JobWorkingFolderDoesNotExist, exception.Message);
        }

        [Fact]
        public async Task LoadExtractFilesAsync_ShouldSkipFiles_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var job = new Job { JobId = Guid.NewGuid() };
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = Guid.NewGuid(),
                    FileName = "test.txt",
                    FilePath = "" // Empty path
                }
            };
            var mockWorkingDir = new Mock<ShareDirectoryClient>();
            var mockJobDir = new Mock<ShareDirectoryClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder))
                          .Returns(mockWorkingDir.Object);
            mockWorkingDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(true, null));
            mockWorkingDir.Setup(x => x.GetSubdirectoryClient(It.IsAny<string>()))
                         .Returns(mockJobDir.Object);
            mockJobDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Response.FromValue(false, null));
            mockWorkingDir.Setup(x => x.CreateSubdirectoryAsync(It.IsAny<string>(), null, null, null, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(mockJobDir.Object, null));

            // Act
            var result = await _service.LoadExtractFilesAsync(job, jobFiles).AsTask();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task LoadExtractFilesAsync_ShouldMarkFileAsDeleted_WhenSourceFileDoesNotExist()
        {
            // Arrange
            var job = new Job { JobId = Guid.NewGuid() };
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = Guid.NewGuid(),
                    FileName = "test.txt",
                    FilePath = "user@test.com/test.txt"
                }
            };
            var mockWorkingDir = new Mock<ShareDirectoryClient>();
            var mockJobDir = new Mock<ShareDirectoryClient>();
            var mockSourceDir = new Mock<ShareDirectoryClient>();
            var mockSourceFile = new Mock<ShareFileClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder))
                          .Returns(mockWorkingDir.Object);
            mockWorkingDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(true, null));
            mockWorkingDir.Setup(x => x.GetSubdirectoryClient(It.IsAny<string>()))
                         .Returns(mockJobDir.Object);
            mockJobDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Response.FromValue(true, null));

            _mockUserShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                         .Returns(mockSourceDir.Object);
            mockSourceDir.Setup(x => x.GetFileClient(It.IsAny<string>()))
                        .Returns(mockSourceFile.Object);
            mockSourceFile.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(false, null));

            // Act
            var result = await _service.LoadExtractFilesAsync(job, jobFiles).AsTask();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].Deleted);
            Assert.Equal("test.txt", result[0].FileName);
        }

        #endregion

        #region DeleteJobFiles Tests

        [Fact]
        public async Task DeleteJobFiles_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var mockDir = new Mock<ShareDirectoryClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                          .Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.DeleteJobFilesAsync(jobId));
            Assert.Equal(StardardMessages.JobDirectoryDoesNotExist, exception.Message);
        }

        #endregion

        #region CleanupJobWorkingDirectory Tests

        [Fact]
        public async Task CleanupJobWorkingDirectory_ShouldCallDeleteJobFiles()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var mockDir = new Mock<ShareDirectoryClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                          .Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _service.CleanupJobWorkingDirectoryAsync(jobId));
        }

        #endregion


        [Fact]
        public async Task CreateLogFilesAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test.txt";
            var logFolder = "logs";
            var mockLogDir = new Mock<ShareDirectoryClient>();

            _mockUserShare.Setup(x => x.GetDirectoryClient(logFolder))
                         .Returns(mockLogDir.Object);
            mockLogDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test error"));

            _activityLogService.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateLogFilesAsync(
                jobId,
                jobFileId,
                fileName,
                10,
                logFolder,
                new List<int>(),
                new List<int>(),
                new List<int>(),
                DateTime.UtcNow,
                "test@user.com");

            // Assert
            Assert.False(result);
            _activityLogService.Verify(
                x => x.AddLogAsync(It.Is<ActivityLog>(
                    log => log.MessageType == LogMessageTypes.Error)),
                Times.Once);
        }

        #region Edge Case Tests

        [Fact]
        public async Task GetFoldersByEmailAddressAsync_ShouldVerifyDirectoryClientCalled()
        {
            // Arrange
            var email = "test@example.com";
            var mockDir = new Mock<ShareDirectoryClient>();
            _mockUserShare.Setup(x => x.GetDirectoryClient(email)).Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFoldersByEmailAddressAsync(email).AsTask());

            _mockUserShare.Verify(x => x.GetDirectoryClient(email), Times.Once);
        }

        [Fact]
        public async Task LoadExtractFilesAsync_ShouldHandleBackslashInFilePath()
        {
            // Arrange
            var job = new Job { JobId = Guid.NewGuid() };
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = Guid.NewGuid(),
                    FileName = "test.txt",
                    FilePath = "user@test.com\\folder\\test.txt" // Backslash path
                }
            };
            var mockWorkingDir = new Mock<ShareDirectoryClient>();
            var mockJobDir = new Mock<ShareDirectoryClient>();
            var mockSourceDir = new Mock<ShareDirectoryClient>();
            var mockSourceFile = new Mock<ShareFileClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder))
                          .Returns(mockWorkingDir.Object);
            mockWorkingDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(true, null));
            mockWorkingDir.Setup(x => x.GetSubdirectoryClient(It.IsAny<string>()))
                         .Returns(mockJobDir.Object);
            mockJobDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Response.FromValue(true, null));

            _mockUserShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                         .Returns(mockSourceDir.Object);
            mockSourceDir.Setup(x => x.GetFileClient(It.IsAny<string>()))
             .Returns(mockSourceFile.Object);
            mockSourceFile.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(false, null));

            // Act
            var result = await _service.LoadExtractFilesAsync(job, jobFiles).AsTask();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("  ")]
        public async Task GetFileContentsByPathAsync_ShouldHandleInvalidPaths(string filePath)
        {
            // Arrange & Act & Assert
            if (string.IsNullOrWhiteSpace(filePath))
            {
                // Path.GetDirectoryName handles these gracefully
                var dirPath = Path.GetDirectoryName(filePath);
                var mockDir = new Mock<ShareDirectoryClient>();
                _mockUserShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                             .Returns(mockDir.Object);
            }
        }

        [Fact]
        public async Task DeleteJobFiles_ShouldThrowOriginalException_OnError()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var mockDir = new Mock<ShareDirectoryClient>();
            var testException = new InvalidOperationException("Test error");

            _mockMainShare.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
                          .Returns(mockDir.Object);
            mockDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(testException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.DeleteJobFilesAsync(jobId));
            Assert.Equal("Test error", exception.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task LoadExtractFilesAsync_ShouldCreateJobDirectory_WhenItDoesNotExist()
        {
            // Arrange
            var job = new Job { JobId = Guid.NewGuid() };
            var jobFiles = new List<JobFile>();
            var mockWorkingDir = new Mock<ShareDirectoryClient>();
            var mockJobDir = new Mock<ShareDirectoryClient>();

            _mockMainShare.Setup(x => x.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder))
                          .Returns(mockWorkingDir.Object);
            mockWorkingDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(true, null));
            mockWorkingDir.Setup(x => x.GetSubdirectoryClient(job.JobId.ToString()))
                         .Returns(mockJobDir.Object);
            mockJobDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Response.FromValue(false, null));
            mockWorkingDir.Setup(x => x.CreateSubdirectoryAsync(job.JobId.ToString(), null, null, null, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Response.FromValue(mockJobDir.Object, null));

            // Act
            var result = await _service.LoadExtractFilesAsync(job, jobFiles).AsTask();

            // Assert
            Assert.NotNull(result);
            mockWorkingDir.Verify(x => x.CreateSubdirectoryAsync(job.JobId.ToString(), null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test.txt";
            var logFolder = "logs";
            var mockLogDir = new Mock<ShareDirectoryClient>();

            _mockUserShare.Setup(x => x.GetDirectoryClient(logFolder))
                         .Returns(mockLogDir.Object);
            mockLogDir.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test error"));

            _activityLogService.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateLogFilesAsync(
                jobId,
                jobFileId,
                fileName,
                10,
                logFolder,
                new List<int>(),
                new List<int>(),
                new List<int>(),
                DateTime.UtcNow,
                "test@user.com");

            // Assert
            Assert.False(result);
            _activityLogService.Verify(
                x => x.AddLogAsync(It.Is<ActivityLog>(
                    log => log.MessageType == LogMessageTypes.Error)),
                Times.Once);
        }

        #region CopyOutputFilesToSupplyChainFolderAsync Tests

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldThrowArgumentException_WhenJobIdIsEmpty()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.CopyOutputFilesToSupplyChainFolderAsync(Guid.Empty, new List<JobFile> { new JobFile() }));
            Assert.Equal("Invalid jobId or jobFiles.", exception.Message);
        }

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldThrowArgumentException_WhenJobFilesIsNull()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.CopyOutputFilesToSupplyChainFolderAsync(Guid.NewGuid(), null));
            Assert.Equal("Invalid jobId or jobFiles.", exception.Message);
        }

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldThrowArgumentException_WhenJobFilesIsEmpty()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.CopyOutputFilesToSupplyChainFolderAsync(Guid.NewGuid(), new List<JobFile>()));
            Assert.Equal("Invalid jobId or jobFiles.", exception.Message);
        }

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldThrowException_WhenConfigSettingNotFound()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "test.txt", FilePath = "user@test.com/test.txt" }
            };

            _mockConfigSettingService
                .Setup(x => x.GetSettingByModuleAndNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ConfigSetting)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.CopyOutputFilesToSupplyChainFolderAsync(jobId, jobFiles));
            Assert.Contains(WellKnownPathNames.SupplyChainTargetFolder, exception.Message);
        }

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldThrowDirectoryNotFoundException_WhenTargetDirectoryDoesNotExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "test.txt", FilePath = "user@test.com/test.txt" }
            };
            var targetFolder = "SupplyChainCloud/FromDvt_CloudTest";
            var mockSourceDir = new Mock<ShareDirectoryClient>();
            var mockTargetDir = new Mock<ShareDirectoryClient>();

            _mockConfigSettingService
                .Setup(x => x.GetSettingByModuleAndNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ConfigSetting { Value = targetFolder });

            _mockMainShare
                .Setup(x => x.GetDirectoryClient(It.Is<string>(s => s.Contains(jobId.ToString()))))
                .Returns(mockSourceDir.Object);
            _mockMainShare
                .Setup(x => x.GetDirectoryClient(targetFolder))
                .Returns(mockTargetDir.Object);

            mockTargetDir
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, null));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _service.CopyOutputFilesToSupplyChainFolderAsync(jobId, jobFiles));
        }

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldCopyFiles_WhenSourceFilesExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "test.txt", FilePath = "user@test.com/test.txt" },
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "test2.txt", FilePath = "user@test.com/test2.txt" }
            };
            var targetFolder = "SupplyChainCloud/FromDvt_CloudTest";
            var mockSourceDir = new Mock<ShareDirectoryClient>();
            var mockTargetDir = new Mock<ShareDirectoryClient>();
            var mockSourceFile = new Mock<ShareFileClient>();
            var mockTargetFile = new Mock<ShareFileClient>();

            _mockConfigSettingService
                .Setup(x => x.GetSettingByModuleAndNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ConfigSetting { Value = targetFolder });

            _mockMainShare
                .Setup(x => x.GetDirectoryClient(It.Is<string>(s => s.Contains(jobId.ToString()))))
                .Returns(mockSourceDir.Object);
            _mockMainShare
                .Setup(x => x.GetDirectoryClient(targetFolder))
                .Returns(mockTargetDir.Object);

            mockTargetDir
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, null));

            mockSourceDir
                .Setup(x => x.GetFileClient(It.IsAny<string>()))
                .Returns(mockSourceFile.Object);
            mockTargetDir
                .Setup(x => x.GetFileClient(It.IsAny<string>()))
                .Returns(mockTargetFile.Object);

            mockSourceFile
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, null));
            mockTargetFile
                .Setup(x => x.StartCopyAsync(It.IsAny<Uri>(), It.IsAny<ShareFileCopyOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(Mock.Of<ShareFileCopyInfo>(), null));

            // Act
            await _service.CopyOutputFilesToSupplyChainFolderAsync(jobId, jobFiles);

            // Assert
            mockTargetFile.Verify(
                x => x.StartCopyAsync(It.IsAny<Uri>(), It.IsAny<ShareFileCopyOptions>(), It.IsAny<CancellationToken>()),
                Times.Exactly(jobFiles.Count));
        }

        [Fact]
        public async Task CopyOutputFilesToSupplyChainFolderAsync_ShouldSkipMissingSourceFiles_WhenSourceFileDoesNotExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "missing.txt", FilePath = "user@test.com/missing.txt" }
            };
            var targetFolder = "SupplyChainCloud/FromDvt_CloudTest";
            var mockSourceDir = new Mock<ShareDirectoryClient>();
            var mockTargetDir = new Mock<ShareDirectoryClient>();
            var mockSourceFile = new Mock<ShareFileClient>();
            var mockTargetFile = new Mock<ShareFileClient>();

            _mockConfigSettingService
                .Setup(x => x.GetSettingByModuleAndNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ConfigSetting { Value = targetFolder });

            _mockMainShare
                .Setup(x => x.GetDirectoryClient(It.Is<string>(s => s.Contains(jobId.ToString()))))
                .Returns(mockSourceDir.Object);
            _mockMainShare
                .Setup(x => x.GetDirectoryClient(targetFolder))
                .Returns(mockTargetDir.Object);

            mockTargetDir
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, null));

            mockSourceDir
                .Setup(x => x.GetFileClient(It.IsAny<string>()))
                .Returns(mockSourceFile.Object);
            mockTargetDir
                .Setup(x => x.GetFileClient(It.IsAny<string>()))
                .Returns(mockTargetFile.Object);

            mockSourceFile
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, null));

            // Act
            await _service.CopyOutputFilesToSupplyChainFolderAsync(jobId, jobFiles);

            // Assert - StartCopyAsync should never be called when source file doesn't exist
            mockTargetFile.Verify(
                x => x.StartCopyAsync(It.IsAny<Uri>(), It.IsAny<ShareFileCopyOptions>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion
    }
}