using DVT.Core.FileLoader;
using DVT.Core.Models;
using DVT.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ServiceTests
{
    public class FileLoadServiceTests
    {
        private readonly Mock<IStorageService> _storageServiceMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly FileLoadService _service;

        public FileLoadServiceTests()
        {
            _storageServiceMock = new Mock<IStorageService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _service = new FileLoadService(_storageServiceMock.Object, _activityLogServiceMock.Object);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadVirFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_vir.txt";
            var fileContent = "Division ID|Local site ID|Receipt Number|PO Number|PO line number|Supplier ID|Part Number|Supplier part Number|Quantity Ordered|Quantity Received|Date Received|Invoice Price Paid|Unit Price|Pure_loaded cost|Currency code|Intra-div|Direct_indirect|PO terms|Freight terms|UOM|Title transfer|Port|Release#|Committed Date|";
            fileContent += "\n0055|VLC-RENO|R000000001|4500002866|00010|2200581|11023171||4.000|4.000|20150619|134.0000|134.0000|P|USD|N|D|NET60|INCO2010 FOB||O||20150619|";
            fileContent += "\n0055|VLC-RENO|R000000002|4500002866|00020|2200581|11023172||2.000|2.000|20150620|200.0000|200.0000|P|USD|N|D|NET60|INCO2010 FOB||O||20150620|";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Vir",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Operations.UploadFiles, result.Operation);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadSupplierFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_supplier.txt";
            var fileContent = "Division ID|Local site ID|Supplier ID|Supplier name|DUNS|Active_inactive|Direct_Indirect|Address Descr|Street|Suite|City|State|Postal Code|County|Country|Addr1|Addr2|Addr3|Addr4|Country code|Global Flag|Main Telephone|Toll Free|Fax|Web site|Supplier Type|";
            fileContent += "\n0055|VLC-RENO|2200581|SUPPLIER A|123456789|Active|Direct|Address 1|123 Main St||Reno|NV|89501|Washoe|USA||||||US|Y|555-1234||555-5678|www.suppliera.com|Type A|";
            fileContent += "\n0055|VLC-RENO|2200582|SUPPLIER B|987654321|Active|Indirect|Address 2|456 Elm St||Reno|NV|89502|Washoe|USA||||||US|Y|555-4321||555-8765|www.supplierb.com|Type B|";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Supplier",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);
            _activityLogServiceMock.Verify(a => a.AddLogAsync(It.IsAny<ActivityLog>()), Times.Once);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadItemFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_item.txt";
            var fileContent = "Division ID|Local site ID|Part Number|Description|Comcode|DRI code|Part status|Direct_indirect|Purch_mfrd|Lead time|Standard cost|Pure_loaded cost|Currency code|UOM|ABC Category|";
            fileContent += "\n0055|VLC-RENO|2000305|GLAND BOX F951 600 304SS||F040|A|D|P|93|264.0500|P|USD|EA|D|";
            fileContent += "\n0055|VLC-RENO|2000306|GLAND BOX F952 600 304SS||F041|A|D|P|120|300.0000|P|USD|EA|D|";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Item",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadInventoryFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_inventory.txt";
            var fileContent = "Division ID|Local site ID|Part Number|Quantity|Standard cost|Total value|UOM|Currency code|Part status|Comcode|DRI code|Description|Inventory date|";
            fileContent += "\n0055|VLC-RENO|2000305|10|264.0500|2640.5000|EA|USD|A||F040|GLAND BOX F951 600 304SS|2024-01-01|";
            fileContent += "\n0055|VLC-RENO|2000306|5|300.0000|1500.0000|EA|USD|A||F041|GLAND BOX F952 600 304SS|2024-01-02|";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Inventory",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadPoFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_po.txt";
            var fileContent = "Division ID|Local site ID|PO Number|Order date|Latest Amendment|Commodity Mgr ID|Supplier ID|Currency code|PO Type|Intra-div|Direct_indirect|PO Terms|Freight Terms|EDI|Order Status|Title transfer|Port|";
            fileContent += "\n0055|VLC-RENO|4500002866|20120622|||2200581|USD|P|N|D|NET60|INCO2010 FOB||O|ORIGIN||";
            fileContent += "\n0055|VLC-RENO|4500002867|20120623|||2200582|USD|P|N|D|NET60|INCO2010 FOB||O|ORIGIN||";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Po",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadPoItemFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_poitem.txt";
            var fileContent = "Division ID|Local site ID|PO Number|PO line Number|Part number|Supplier part number|Description|Contract ID|Unit cost|Pure_loaded cost|Ordered value|Quantity ordered|Quantity returned|Committed date|Requested date|Order status|Currency code|UOM|Qty left to receive|Value left to receive|Release#|";
            fileContent += "\n0055|VLC-RENO|4500002866|00010|11023171||GLAND BOX F951 600 304SS||134.0000|P|536.0000|4.000|0.000|20150619|20150619|O|USD|EA|0.000|0.000||";
            fileContent += "\n0055|VLC-RENO|4500002866|00020|11023172||GLAND BOX F952 600 304SS||200.0000|P|400.0000|2.000|0.000|20150620|20150620|O|USD|EA|0.000|0.000||";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "PoItem",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadUomFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_uom.txt";
            var fileContent = "Division ID|Local Site ID|Part Number|Local UOM|Base UOM|Conversion Rate|";
            fileContent += "\nVID1|VIDEOTON-1|C55637-0001|M|EA|1|";
            fileContent += "\nVID1|VIDEOTON-1|C55637-0002|M|EA|1|";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Uom",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldLoadMpnFileSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_mpn.txt";
            var fileContent = "Division ID|Local Site ID|Part Number|Local Manufacturer ID|Manufacture ID|Manufacture Name|Manufacturer Part Number|Object ID|MPN Type|";
            fileContent += "\n0016|NI-PEN|100013A-01|49337|1724|QORVO|TGA2526|4CCD851E3F8FF78947BD269D07AB573031D4ADD1|P|";
            fileContent += "\n0016|NI-PEN|100013A-02|49337|1724|QORVO|TGA2527|4CCD851E3F8FF78947BD269D07AB573031D4ADD2|P|";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Mpn",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.True(result.FileLoadResults[0].Success);

            //Should be two rows of data excluding header
            Assert.Equal(2, result.FileLoadResults[0].DataRows.Count);
            Assert.Equal(1, result.FileLoadResults[0].DataRows[0].RowNumber);
            Assert.Equal(2, result.FileLoadResults[0].DataRows[1].RowNumber);
        }

        [Fact]
        public async Task LoadFile_ShouldReturnErrorForUnknownFileType()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test_unknown.txt";
            var fileContent = "header1|header2\nvalue1|value2";
            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "UnknownType",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.Equal("Unknown file template: UnknownType", result.FileLoadResults[0].Message);
            _activityLogServiceMock.Verify(a => a.AddLogAsync(It.IsAny<ActivityLog>()), Times.Once);
        }

        [Fact]
        public async Task LoadFile_ShouldLogErrorWhenFileContentIsEmpty()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "empty.txt";
            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(string.Empty);

            _activityLogServiceMock
                .Setup(a => a.AddLogAsync(It.IsAny<ActivityLog>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Vir",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.False(result.FileLoadResults[0].Success);
            Assert.Contains(StardardMessages.FileContentIsEmpty, result.FileLoadResults[0].Message);
            _activityLogServiceMock.Verify(a => a.AddLogAsync(It.IsAny<ActivityLog>()), Times.Once);
        }

        [Fact]
        public async Task LoadFile_ShouldLogErrorWhenFileContentIsWhitespace()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "whitespace.txt";
            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync("   ");

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Vir",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.False(result.FileLoadResults[0].Success);
            Assert.Contains(StardardMessages.FileContentIsEmpty, result.FileLoadResults[0].Message);
        }

        [Fact]
        public async Task LoadFile_ShouldHandleMultipleFilesSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var fileContent = "header1|header2\nvalue1|value2";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = Guid.NewGuid(),
                        FileType = "Vir",
                        FileName = "test_vir.txt",
                        FilePath = "path1"
                    },
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = Guid.NewGuid(),
                        FileType = "Supplier",
                        FileName = "test_supplier.txt",
                        FilePath = "path2"
                    },
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = Guid.NewGuid(),
                        FileType = "Item",
                        FileName = "test_item.txt",
                        FilePath = "path3"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.FileLoadResults.Count);
            Assert.All(result.FileLoadResults, r => Assert.True(r.Success));
            _activityLogServiceMock.Verify(a => a.AddLogAsync(It.IsAny<ActivityLog>()), Times.Exactly(3));
        }

        [Fact]
        public async Task LoadFile_ShouldHandleMixedSuccessAndFailureFiles()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var validContent = "header1|header2\nvalue1|value2";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, "valid.txt"))
                .ReturnsAsync(validContent);

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, "empty.txt"))
                .ReturnsAsync(string.Empty);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = Guid.NewGuid(),
                        FileType = "Vir",
                        FileName = "valid.txt",
                        FilePath = "path1"
                    },
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = Guid.NewGuid(),
                        FileType = "Item",
                        FileName = "empty.txt",
                        FilePath = "path2"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.FileLoadResults.Count);
            Assert.True(result.FileLoadResults[0].Success);
            Assert.False(result.FileLoadResults[1].Success);
        }

        [Fact]
        public async Task LoadFile_ShouldSetJobFileIdCorrectly()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test.txt";
            var fileContent = "header1|header2\nvalue1|value2";
            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Vir",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.Equal(jobFileId, result.FileLoadResults[0].JobFileId);
        }

        [Fact]
        public async Task LoadFile_ShouldLogActivityForEachFile()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var fileContent = "header1|header2\nvalue1|value2";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest {JobId = jobId, JobFileId = Guid.NewGuid(), FileType = "Vir", FileName = "file1.txt", FilePath = "path1" },
                    new FileLoadRequest {JobId = jobId, JobFileId = Guid.NewGuid(), FileType = "Item", FileName = "file2.txt", FilePath = "path2" }
                }
            };

            // Act
            await _service.LoadFile(jobLoad);

            // Assert
            _activityLogServiceMock.Verify(
                a => a.AddLogAsync(It.Is<ActivityLog>(log =>
                    log.Entity == DVTEntities.JobFile &&
                    log.CreateBy == "user@test.com")),
                Times.Exactly(2));
        }

        [Fact]
        public async Task LoadFile_ShouldHandleStorageServiceException()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobFileId = Guid.NewGuid();
            var fileName = "test.txt";

            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(jobId, fileName))
                .ThrowsAsync(new Exception("Storage error"));

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest
                    {
                        JobId = jobId,
                        JobFileId = jobFileId,
                        FileType = "Vir",
                        FileName = fileName,
                        FilePath = "somepath"
                    }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.FileLoadResults);
            Assert.False(result.FileLoadResults[0].Success);
            Assert.Contains("Storage error", result.FileLoadResults[0].Message);
        }

        [Fact]
        public async Task LoadFile_ShouldReturnOperationUploadFiles()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var fileContent = "header1|header2\nvalue1|value2";
            _storageServiceMock
                .Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(fileContent);

            var jobLoad = new JobLoad
            {
                JobId = jobId,
                UserEmail = "user@test.com",
                FileList = new List<FileLoadRequest>
                {
                    new FileLoadRequest { JobId = jobId, JobFileId = Guid.NewGuid(), FileType = "Vir", FileName = "test.txt", FilePath = "path" }
                }
            };

            // Act
            var result = await _service.LoadFile(jobLoad);

            // Assert
            Assert.Equal(Operations.UploadFiles, result.Operation);
        }
    }
}