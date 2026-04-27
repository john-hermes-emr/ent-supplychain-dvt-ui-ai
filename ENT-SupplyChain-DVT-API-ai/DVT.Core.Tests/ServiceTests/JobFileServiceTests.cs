using DVT.Core;
using DVT.Core.Models;
using DVT.Core.Services;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ServiceTests
{
    public class JobFileServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IStorageService> _storageServiceMock;
        private readonly JobFileService _service;

        public JobFileServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _storageServiceMock = new Mock<IStorageService>();
            _service = new JobFileService(
                _unitOfWorkMock.Object,
                _activityLogServiceMock.Object,
                _storageServiceMock.Object);
        }

        #region GetJobFileByIdAsync Tests

        [Fact]
        public async Task GetJobFileByIdAsync_ShouldReturnJobFile_WhenFileExists()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            var expectedJobFile = new JobFile
            {
                JobFileId = jobFileId,
                FileName = "test.txt",
                FileType = "Vir"
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync(expectedJobFile);

            // Act
            var result = await _service.GetJobFileByIdAsync(jobFileId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jobFileId, result.JobFileId);
            Assert.Equal("test.txt", result.FileName);
        }

        [Fact]
        public async Task GetJobFileByIdAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync((JobFile)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetJobFileByIdAsync(jobFileId));
        }

        #endregion

        #region GetJobFilesByJobIdAsync Tests

        [Fact]
        public async Task GetJobFilesByJobIdAsync_ShouldReturnFiles_WhenFilesExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var expectedFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file1.txt" },
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file2.txt" }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(expectedFiles);

            // Act
            var result = await _service.GetJobFilesByJobIdAsync(jobId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetJobFilesByJobIdAsync_ShouldThrowException_WhenNoFilesExist()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync((IEnumerable<JobFile>)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetJobFilesByJobIdAsync(jobId));
        }

        #endregion

        #region UpdateJobFileValidationResultAsync Tests

        [Fact]
        public async Task UpdateJobFileValidationResultAsync_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            var jobFile = new JobFile
            {
                JobFileId = jobFileId,
                FileName = "test.txt",
                Status = WellKnownFileStatuses.InProgress
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync(jobFile);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateJobFileValidationResultAsync(
                jobFileId,
                "validation messages",
                "validation stats",
                WellKnownFileStatuses.Validated,
                "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(Operations.SetJobFileValidationResult, result.Operation);
            Assert.Equal(WellKnownFileStatuses.Validated, jobFile.Status);
            _activityLogServiceMock.Verify(x => x.AddLogAsync(It.IsAny<ActivityLog>()), Times.Once);
        }

        [Fact]
        public async Task UpdateJobFileValidationResultAsync_ShouldThrowException_WhenFileNotFound()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync((JobFile)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.UpdateJobFileValidationResultAsync(
                    jobFileId, "msg", "stats", "Validated", "user@test.com"));
        }

        #endregion

        #region UpdateJobFileStatusAsync Tests

        [Fact]
        public async Task UpdateJobFileStatusAsync_ShouldUpdateStatus_WhenValidStatusProvided()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            var jobFile = new JobFile
            {
                JobFileId = jobFileId,
                FileName = "test.txt",
                Status = WellKnownFileStatuses.New
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync(jobFile);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateJobFileStatusAsync(
                jobFileId,
                WellKnownFileStatuses.Validated,
                "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(WellKnownFileStatuses.Validated, jobFile.Status);
            Assert.Equal(StardardMessages.JobFileStatusUpdatedSuccessfully, result.Message);
        }

        [Fact]
        public async Task UpdateJobFileStatusAsync_ShouldThrowException_WhenInvalidStatus()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            var jobFile = new JobFile { JobFileId = jobFileId };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync(jobFile);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.UpdateJobFileStatusAsync(jobFileId, "InvalidStatus", "user@test.com"));
        }


        #endregion

        #region BatchUpdateJobFilesStatusAsync Tests

        [Fact]
        public async Task BatchUpdateJobFilesStatusAsync_ShouldUpdateMultipleFiles()
        {
            // Arrange
            var jobFileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = jobFileIds[0], FileName = "file1.txt" },
                new JobFile { JobFileId = jobFileIds[1], FileName = "file2.txt" }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobFileIdsNoValidationAsync(jobFileIds))
                .ReturnsAsync(jobFiles);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.BatchUpdateJobFilesStatusAsync(
                jobFileIds,
                WellKnownFileStatuses.Validated,
                "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.All(jobFiles, f => Assert.Equal(WellKnownFileStatuses.Validated, f.Status));
        }

        [Fact]
        public async Task BatchUpdateJobFilesStatusAsync_ShouldThrowException_WhenNoFilesFound()
        {
            // Arrange
            var jobFileIds = new List<Guid> { Guid.NewGuid() };
            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobFileIdsAsync(jobFileIds))
                .ReturnsAsync(new List<JobFile>());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.BatchUpdateJobFilesStatusAsync(jobFileIds, "Validated", "user@test.com"));
        }

        #endregion

        #region UpdateJobFilesStatusByJobIdAsync Tests

        [Fact]
        public async Task UpdateJobFilesStatusByJobIdAsync_ShouldUpdateAllFilesForJob()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file1.txt" },
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file2.txt" }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdNoValidationAsync(jobId))
                .ReturnsAsync(jobFiles);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogsAsync(It.IsAny<List<ActivityLog>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateJobFilesStatusByJobIdAsync(
                jobId,
                WellKnownFileStatuses.InProgress,
                "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.All(jobFiles, f => Assert.Equal(WellKnownFileStatuses.InProgress, f.Status));
            _activityLogServiceMock.Verify(x => x.AddLogsAsync(It.IsAny<List<ActivityLog>>()), Times.Once);
        }

        #endregion

        #region DeleteJobFilesAsync Tests

        [Fact]
        public async Task DeleteJobFilesAsync_ShouldMarkFilesAsDeleted()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file1.txt", Deleted = false },
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file2.txt", Deleted = false }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdNoValidationAsync(jobId))
                .ReturnsAsync(jobFiles);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogsAsync(It.IsAny<List<ActivityLog>>()))
                .Returns(Task.CompletedTask);
            _storageServiceMock.Setup(x => x.DeleteJobFilesAsync(jobId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteJobFilesAsync(jobId, "user@test.com", false);

            // Assert
            Assert.True(result.Success);
            Assert.All(jobFiles, f => Assert.True(f.Deleted));
            _storageServiceMock.Verify(x => x.DeleteJobFilesAsync(jobId), Times.Once);
        }

        [Fact]
        public async Task DeleteJobFilesAsync_ShouldUseRefreshOperation_WhenRefreshIsTrue()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid(), FileName = "file1.txt" }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdNoValidationAsync(jobId))
                .ReturnsAsync(jobFiles);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogsAsync(It.IsAny<List<ActivityLog>>()))
                .Returns(Task.CompletedTask);
            _storageServiceMock.Setup(x => x.DeleteJobFilesAsync(jobId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteJobFilesAsync(jobId, "user@test.com", true);

            // Assert
            Assert.True(result.Success);
            _activityLogServiceMock.Verify(x => x.AddLogsAsync(
                It.Is<List<ActivityLog>>(logs => logs.Any(l => l.Message == Operations.RefreshAndDeleteJobFile))),
                Times.Once);
        }

        #endregion

        #region GetJobFileValidationMessageByJobIdAndJobFileIdAsync Tests

        [Fact]
        public async Task GetJobFileValidationMessageByJobIdAndJobFileIdAsync_ShouldReturnNoError_WhenNoValidationMessages()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = jobFileId,
                    FileName = "test.txt",
                    ValidationMessages = null,
                    LoadDate = DateTime.UtcNow
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);

            // Act
            var result = await _service.GetJobFileValidationMessageByJobIdAndJobFileIdAsync(
                jobId, jobFileId, "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(StardardMessages.NoErrorFound, result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetJobFileValidationMessageByJobIdAndJobFileIdAsync_ShouldThrowException_WhenJobFileNotFound()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile { JobFileId = Guid.NewGuid() } // Different ID
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetJobFileValidationMessageByJobIdAndJobFileIdAsync(
                    jobId, jobFileId, "user@test.com"));
        }

        #endregion

        #region GenerateJobFileErrorReportByJobIdAndJobFileIdAsync Tests

        [Fact]
        public async Task GenerateJobFileErrorReportByJobIdAndJobFileIdAsync_ShouldReturnByteArray()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = jobFileId,
                    FileName = "test.txt",
                    ValidationMessages = null,
                    LoadDate = DateTime.UtcNow,
                    FileType = "Vir",
                    TableName = "vir"
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);

            // Act
            var result = await _service.GenerateJobFileErrorReportByJobIdAndJobFileIdAsync(
                jobId, jobFileId, "user@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        #endregion

        #region GetJobValidationStatsByJobIdAndJobFileIdAsync Tests

        [Fact]
        public async Task GetJobValidationStatsByJobIdAndJobFileIdAsync_ShouldReturnStats_ForVirFile()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var stats = new FileCalculateStatistics_Vir
            {
                TotalRecords = 100,
                QuantityOrderedMin = "1",
                QuantityOrderedMax = "1000"
            };

            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = jobFileId,
                    FileName = "test.txt",
                    FileType = FileTypes.Vir,
                    TableName = "vir",
                    ValidationStats = JsonConvert.SerializeObject(stats),
                    LoadDate = DateTime.UtcNow
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);

            // Act
            var result = await _service.GetJobValidationStatsByJobIdAndJobFileIdAsync(
                jobId, jobFileId, "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var statsResult = result.Data as FileValidationStatsResult;
            Assert.NotNull(statsResult);
            Assert.Equal(FileTypes.Vir, statsResult.FileType);
        }

        [Theory]
        [InlineData("Item")]
        [InlineData("Supplier")]
        [InlineData("Inventory")]
        [InlineData("Po")]
        [InlineData("PoItem")]
        [InlineData("Uom")]
        [InlineData("Mpn")]
        public async Task GetJobValidationStatsByJobIdAndJobFileIdAsync_ShouldHandleDifferentFileTypes(string fileType)
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = jobFileId,
                    FileName = "test.txt",
                    FileType = fileType,
                    TableName = fileType.ToLower(),
                    ValidationStats = "{}",
                    LoadDate = DateTime.UtcNow
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);

            // Act
            var result = await _service.GetJobValidationStatsByJobIdAndJobFileIdAsync(
                jobId, jobFileId, "user@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetJobValidationStatsByJobIdAndJobFileIdAsync_ShouldHandleException()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = jobFileId,
                    FileName = "test.txt",
                    FileType = FileTypes.Vir,
                    ValidationStats = "invalid json",
                    LoadDate = DateTime.UtcNow
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetJobValidationStatsByJobIdAndJobFileIdAsync(
                jobId, jobFileId, "user@test.com");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Exception);
            _activityLogServiceMock.Verify(x => x.AddLogAsync(
                It.Is<ActivityLog>(log => log.MessageType == LogMessageTypes.Error)),
                Times.Once);
        }

        #endregion

        #region GenerateJobFileStatsReportByJobIdAndJobFileIdAsync Tests

        [Fact]
        public async Task GenerateJobFileStatsReportByJobIdAndJobFileIdAsync_ShouldReturnByteArray()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var stats = new FileCalculateStatistics_Vir
            {
                TotalRecords = 100
            };

            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = jobFileId,
                    FileName = "test.txt",
                    FileType = FileTypes.Vir,
                    TableName = "vir",
                    ValidationStats = JsonConvert.SerializeObject(stats),
                    LoadDate = DateTime.UtcNow
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdAsync(jobId))
                .ReturnsAsync(jobFiles);

            // Act
            var result = await _service.GenerateJobFileStatsReportByJobIdAndJobFileIdAsync(
                jobId, jobFileId, "user@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task UpdateJobFileStatusAsync_ShouldSetUpdateDate()
        {
            // Arrange
            var jobFileId = Guid.NewGuid();
            var jobFile = new JobFile
            {
                JobFileId = jobFileId,
                FileName = "test.txt",
                UpdateDate = DateTime.UtcNow.AddDays(-1)
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync(jobFile);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);

            var beforeUpdate = DateTime.UtcNow;

            // Act
            await _service.UpdateJobFileStatusAsync(jobFileId, "Validated", "user@test.com");

            // Assert
            Assert.True(jobFile.UpdateDate >= beforeUpdate);
        }

        [Fact]
        public async Task UpdateJobFileValidationResultAsync_ShouldSetAllFields()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var jobFile = new JobFile { JobId = jobId, JobFileId = jobFileId, FileName = "test.txt" };
            var validationMsg = "test validation";
            var validationStats = "test stats";
            var status = WellKnownFileStatuses.Validated;
            var updateBy = "user@test.com";
            
            _unitOfWorkMock.Setup(x => x.JobFiles.GetByIdAsync(jobFileId))
                .ReturnsAsync(jobFile);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask);
            _storageServiceMock.Setup(x => x.CreateTextFileInWorkingFolderAsync(jobId, "test.txt", validationMsg));

            // Act
            await _service.UpdateJobFileValidationResultAsync(
                jobFileId, validationMsg, validationStats, status, updateBy);

            // Assert
            // We're not setting the validation messages to save to the database since they are going to a file in the working directory
            Assert.Equal(validationStats, jobFile.ValidationStats);
            Assert.Equal(status, jobFile.Status);
            Assert.Equal(updateBy, jobFile.UpdateBy);
            Assert.NotNull(jobFile.UpdateDate);
            Assert.NotNull(jobFile.LoadDate);
        }

        [Fact]
        public async Task BatchUpdateJobFilesStatusAsync_ShouldThrowException_WhenEmptyList()
        {
            // Arrange
            var emptyList = new List<Guid>();
            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobFileIdsNoValidationAsync(emptyList))
                .ReturnsAsync(new List<JobFile>());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.BatchUpdateJobFilesStatusAsync(emptyList, "Validated", "user@test.com"));
        }

        [Fact]
        public async Task DeleteJobFilesAsync_ShouldUpdateAllFileMetadata()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var updateBy = "user@test.com";
            var jobFiles = new List<JobFile>
            {
                new JobFile
                {
                    JobFileId = Guid.NewGuid(),
                    FileName = "file1.txt",
                    Deleted = false,
                    UpdateDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            _unitOfWorkMock.Setup(x => x.JobFiles.GetJobFilesByJobIdNoValidationAsync(jobId))
                .ReturnsAsync(jobFiles);
            _unitOfWorkMock.Setup(x => x.CommitAsync())
                .ReturnsAsync(1);
            _activityLogServiceMock.Setup(x => x.AddLogsAsync(It.IsAny<List<ActivityLog>>()))
                .Returns(Task.CompletedTask);
            _storageServiceMock.Setup(x => x.DeleteJobFilesAsync(jobId))
                .Returns(Task.CompletedTask);

            var beforeDelete = DateTime.UtcNow;

            // Act
            await _service.DeleteJobFilesAsync(jobId, updateBy, false);

            // Assert
            var file = jobFiles.First();
            Assert.True(file.Deleted);
            Assert.Equal(updateBy, file.UpdateBy);
            Assert.True(file.UpdateDate >= beforeDelete);
            Assert.True(file.LoadDate >= beforeDelete);
        }

        #endregion
    }
}