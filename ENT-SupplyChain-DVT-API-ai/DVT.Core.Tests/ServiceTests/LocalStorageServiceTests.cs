/*
using Azure;
using Azure.Storage.Files.Shares;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DVT.Core.Models;
using DVT.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ServiceTests
{
    [Collection(nameof(LocalStorageServiceCollection))]
    public class LocalStorageServiceTests : IClassFixture<LocalStorageServiceFixture>
    {
        LocalStorageServiceFixture _fixture;

        public LocalStorageServiceTests(LocalStorageServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Test_GetFoldersByEmailAddressAsync_ReturnsExpectedFolders()
        {
            // Arrange
            var localStorageService = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);

            // Act
            var folders = await localStorageService.GetFoldersByEmailAddressAsync(_fixture.TestUserEmailAddress);

            // Assert
            Assert.NotNull(folders);
            Assert.Equal(3, folders?.Folders.Count());
        }

        [Fact]
        public async Task GetFoldersByEmailAddressAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var localStorageService = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            string nonExistentEmailAddress = "nonexistent@emerson.com";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await localStorageService.GetFoldersByEmailAddressAsync(nonExistentEmailAddress);
            });

            Assert.Contains("User's folder does not exist", exception.Message);
        }

        [Fact]
        public async Task GetFilesInDirectoryAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var localStorageService = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            string nonExistentDirectoryPath = "test-path";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await localStorageService.GetFilesInDirectoryAsync(nonExistentDirectoryPath);
            });

            Assert.Contains("Folder 'test-path' does not exist.", exception.Message);
        }

        [Fact]
        public async Task GetFileContentsByPathAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFileContentsByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task GetFileContentsByPathAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var filePath = "test@emerson.com/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetFileContentsByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task GetFileContentsByPathAsync_ShouldReturnFileContents_WhenFileExists()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var filePath = Path.Combine(_fixture.TestUserEmailAddress, "Load Folder", "testfile.txt");
            var expectedContent = "Header";

            // Act
            var content = await _service.GetFileContentsByPathAsync(filePath);

            // Assert
            Assert.Contains(expectedContent, content);
        }

        [Fact]
        public async Task GetWorkingFileContentsAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var jobId = _fixture.JobId;
            var fileName = "test.txt";
            var mockDir = new Mock<ShareDirectoryClient>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetWorkingFileContentsAsync(jobId, fileName).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task GetWorkingFileContentsAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var jobId = _fixture.JobId;
            var fileName = "test.txt";
            var mockDir = new Mock<ShareDirectoryClient>();
            var mockFile = new Mock<ShareFileClient>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetWorkingFileContentsAsync(jobId, fileName).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task GetWorkingFileContentsAsync_ShouldReturnFileContents_WhenFileExists()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var jobId = _fixture.JobId;
            var fileName = "input.txt";
            var expectedContent = "Header";
            // Act
            var content = await _service.GetWorkingFileContentsAsync(jobId, fileName);
            // Assert
            Assert.Contains(expectedContent, content);
        }

        [Fact]
        public async Task AnalyzeFileByPathAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.AnalyzeFileByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task AnalyzeFileByPathAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var filePath = "folder/file.txt";
            var dirPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.AnalyzeFileByPathAsync(filePath).AsTask());
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task AnalyzeFileByPathAsync_ShouldReturnAnalysisResult_WhenFileExists()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
            var filePath = Path.Combine(_fixture.TestUserEmailAddress, "Load Folder", "testfile.txt");
            var expectedLineCount = 2;

            // Act
            Models.FileInfo analysisResult = await _service.AnalyzeFileByPathAsync(filePath);

            // Assert
            Assert.NotNull(analysisResult);
            Assert.Equal(expectedLineCount, analysisResult.RecordCount);
            Assert.Equal(filePath, analysisResult.FilePath);
            Assert.Equal("testfile.txt", analysisResult.FileName);
            Assert.Equal("file:///C:/DVT/UserShare/test@emerson.com/Load Folder/testfile.txt", analysisResult.FileUri);
            Assert.Equal("Header\r\nRow1\r\nRow2", analysisResult.FileContent);
        }


        [Fact]
        public async Task LoadExtractFilesAsync_ShouldSkipFiles_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var _service = new LocalStorageService(_fixture.ActivityLogServiceMock.Object);
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
        
    }

    public class LocalStorageServiceFixture : IDisposable
    {
        private readonly string _workingFolderRootPath = @"C:\DVT\JobWorkingFolder";
        private readonly string _archiveFolderRootPath = @"C:\DVT\JobArchivesFolder";
        private readonly string _userShareRootPath = @"C:\DVT\UserShare";
        private readonly string _documentsFolderRootPath = @"C:\DVT\Documents";

        public string TestUserEmailAddress { get; private set; } = "test@emerson.com";
        public string TestUserEmailAddressEmpty { get; private set; } = "test@emerson.com-empty";


        public Mock<IActivityLogService> ActivityLogServiceMock { get; private set; }

        public Guid JobId { get; private set; } = Guid.NewGuid();

        public LocalStorageServiceFixture()
        {
            ActivityLogServiceMock = new Mock<IActivityLogService>();

            //Check if the directories exist, if not create them
            if (!Directory.Exists(_workingFolderRootPath))
            {
                Directory.CreateDirectory(_workingFolderRootPath);
            }

            if (!Directory.Exists(_archiveFolderRootPath))
            {
                Directory.CreateDirectory(_archiveFolderRootPath);
            }

            if (!Directory.Exists(_userShareRootPath))
            {
                Directory.CreateDirectory(_userShareRootPath);
            }

            if (!Directory.Exists(_documentsFolderRootPath))
            {
                Directory.CreateDirectory(_documentsFolderRootPath);
            }

            //Create a user folder in the user share directory for the test email address
            string userFolderPath = Path.Combine(_userShareRootPath, TestUserEmailAddress);
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);

                // Create some test folders for the user
                Directory.CreateDirectory(Path.Combine(userFolderPath, "Load Folder"));
                Directory.CreateDirectory(Path.Combine(userFolderPath, "Log Folder"));
                Directory.CreateDirectory(Path.Combine(userFolderPath, "Production Folder"));

                // Create some test files in the Load Folder
                File.WriteAllText(Path.Combine(userFolderPath, "Load Folder", "testfile.txt"), "Header\r\nRow1\r\nRow2");
            }

            //Create a user folder in the user share directory for the test email address
            string emptyUserFolderPath = Path.Combine(_userShareRootPath, TestUserEmailAddressEmpty);
            if (!Directory.Exists(emptyUserFolderPath))
            {
                Directory.CreateDirectory(emptyUserFolderPath);

                // Create some test folders for the user
                Directory.CreateDirectory(Path.Combine(emptyUserFolderPath, "Load Folder"));
            }

            //Create a test job working folder with some test files
            string testJobFolderPath = Path.Combine(_workingFolderRootPath, JobId.ToString());
            if (!Directory.Exists(testJobFolderPath))
            {
                Directory.CreateDirectory(testJobFolderPath);

                // Create some test files in the job folder
                File.WriteAllText(Path.Combine(testJobFolderPath, "input.txt"), "Header\r\nRow1\r\nRow2");
                File.WriteAllText(Path.Combine(testJobFolderPath, "input2.txt"), "Header\r\nRow1\r\nRow2");
            }
        }

        public void Dispose()
        {
            //Delete all the folders and files in the jobworkingfolder
            if (Directory.Exists(_workingFolderRootPath))
            {
                Directory.Delete(_workingFolderRootPath, true);
            }

            //Delete all the test user folders and files in the user share directory
            string userFolderPath = Path.Combine(_userShareRootPath, TestUserEmailAddress);
            if (Directory.Exists(userFolderPath))
            {
                Directory.Delete(userFolderPath, true);
            }
        }
    }

    [CollectionDefinition("LocalStorageServiceTestsCollection")]
    public class LocalStorageServiceCollection : ICollectionFixture<LocalStorageServiceFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}


*/