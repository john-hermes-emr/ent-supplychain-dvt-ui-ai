using DVT.Core.FileLoader;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using Moq;
using DVT.Core.FileLoader;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.LoaderTests
{
    public class InventoryFileFixture : IDisposable
    {
        public string SampleGoodInventoryFileContent { get; private set; }
        public string SampleBadInventoryEmptyLines { get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }

        public InventoryFileFixture()
        {
            SampleGoodInventoryFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_inv.txt");
            SampleBadInventoryEmptyLines = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_inv_empty_lines.txt");

            SampleBadFileExtraColumnsContent = "Division ID|Local site ID|Part Number|Quantity|Standard cost|Total value|UOM|Currency code|Part status|Comcode|DRI code|Description|Inventory date|" +
                "\r\n0055|VLC-RENO|11121502|4|0.8000|3.2000|EA|USD|A||W090|NUT HEX 1-8 PLTD GR-5|20251031|" +
                "\r\n0055|VLC-RENO|11121507|||28|13.8200|386.9600|EA|USD|A||W090|NUT HEX JAM 2-1/2-12 PLTD GR-5|20251031|" + //Added two pipes after part number to break the quantity parsing
                "\r\n0055|VLC-RENO|11121506|33|0.1500|4.9500|EA|USD|A||W100|SCW 3/8-16 X1 SHCS PLTD A574|20251031|";

            SampleBadFileMissingColumnsContent = "Division ID|Local site ID|Part Number|Quantity|Standard cost|Total value|UOM|Currency code|Part status|Comcode|DRI code|Description|Inventory date|" +
                "\r\n0055|VLC-RENO|11121502|4|0.8000|3.2000|EA|USD|A||W090|NUT HEX 1-8 PLTD GR-5|20251031|" +
                "\r\n0055|VLC-RENO|11121507|28|13.8200386.9600|EA|USD|A||W090|NUT HEX JAM 2-1/2-12 PLTD GR-5|20251031|" + //Removed a pipe between quantity and standard cost to break the column parsing
                "\r\n0055|VLC-RENO|11121506|33|0.1500|4.9500|EA|USD|A||W100|SCW 3/8-16 X1 SHCS PLTD A574|20251031|";
        }
        public void Dispose()
        {
            SampleGoodInventoryFileContent = string.Empty;
            SampleBadInventoryEmptyLines = string.Empty;
        }
    }

    public class InventoryFileLoaderTests : IClassFixture<InventoryFileFixture>
    {
        InventoryFileFixture _fixture;

        public InventoryFileLoaderTests(InventoryFileFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GoodFile_ColumnCount()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodInventoryFileContent);

            InventoryFileLoader loader = new InventoryFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_inventory.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Asset
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(13, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodInventoryFileContent);

            InventoryFileLoader loader = new InventoryFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_inventory.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Asset
            Assert.True(result.Success);
            Assert.Equal(9, result.DataRows.Count);
        }

        [Fact]
        public async Task GoodFile_VerifyRowData()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodInventoryFileContent);

            InventoryFileLoader loader = new InventoryFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_inventory.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<InventoryDataRow>().ToList();

            //Asset
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber); 
            Assert.Equal("0055", testRow.DivisionId);
            Assert.Equal("VLC-RENO", testRow.LocalSiteId);
            Assert.Equal("11121502", testRow.PartNumber);
            Assert.Equal(4, testRow.Quantity.HasValue ? testRow.Quantity.Value : 0);
            Assert.Equal(ErrorTypes.None, testRow.QuantityError);
            Assert.Equal(8E-1, testRow.StandardCost.HasValue ? testRow.StandardCost.Value : 0);
            Assert.Equal(ErrorTypes.None, testRow.StandardCostError);
            Assert.Equal(32E-1, testRow.TotalValue.HasValue ? testRow.TotalValue.Value : 0);
            Assert.Equal(ErrorTypes.None, testRow.TotalValueError);
            Assert.Equal("EA", testRow.UOM);
            Assert.Equal("USD", testRow.CurrencyCode);
            Assert.Equal("A", testRow.PartStatus);
            Assert.Equal("", testRow.Comcode);
            Assert.Equal("W090", testRow.DRICode);
            Assert.Equal("NUT HEX 1-8 PLTD GR-5", testRow.Description);
            Assert.Equal("20251031", testRow.InventoryDateStr);
            Assert.Equal("10/31/2025", testRow.InventoryDate?.ToString("MM/dd/yyyy"));
            Assert.Equal("", testRow.InventoryDateError);


            testRow = dataRows[1];
            Assert.Equal(2, testRow.RowNumber); //Second row in the file (first data row)
            Assert.Equal("0055", testRow.DivisionId);
            Assert.Equal("VLC-RENO", testRow.LocalSiteId);
            Assert.Equal("11121507", testRow.PartNumber);
            Assert.Equal(28, testRow.Quantity.HasValue ? testRow.Quantity.Value : 0);
            Assert.Equal(ErrorTypes.None, testRow.QuantityError);
            Assert.Equal(1382E-2, testRow.StandardCost.HasValue ? testRow.StandardCost.Value : 0);
            Assert.Equal(ErrorTypes.None, testRow.StandardCostError);
            Assert.Equal(38696E-2, testRow.TotalValue.HasValue ? testRow.TotalValue.Value : 0);
            Assert.Equal(ErrorTypes.None, testRow.TotalValueError);
            Assert.Equal("EA", testRow.UOM);
            Assert.Equal("USD", testRow.CurrencyCode);
            Assert.Equal("A", testRow.PartStatus);
            Assert.Equal("", testRow.Comcode);
            Assert.Equal("W090", testRow.DRICode);
            Assert.Equal("NUT HEX JAM 2-1/2-12 PLTD GR-5", testRow.Description);
            Assert.NotNull(testRow.InventoryDate);
            Assert.Equal("20251031", testRow.InventoryDateStr);
            Assert.Equal("10/31/2025", testRow.InventoryDate?.ToString("MM/dd/yyyy"));
            Assert.Equal("", testRow.InventoryDateError);
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadInventoryEmptyLines);

            InventoryFileLoader loader = new InventoryFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_inventory_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(13, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((InventoryDataRow)result.DataRows[2]).DivisionId));
            Assert.True(string.IsNullOrEmpty(((InventoryDataRow)result.DataRows[3]).DivisionId));
        }

        [Fact]
        public async Task BadFile_MissingColumns()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadFileMissingColumnsContent);

            InventoryFileLoader loader = new InventoryFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_inventory_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(13, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the StandardCost field is shifted to the right and should fail to parse, resulting in a null value.            
            Assert.Null((result.DataRows[1] as InventoryDataRow)?.StandardCost);
            Assert.Equal(ErrorTypes.InvalidFormat, (result.DataRows[1] as InventoryDataRow)?.StandardCostError);
        }

        [Fact]
        public async Task BadFile_ExtraColumns()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadFileExtraColumnsContent);

            InventoryFileLoader loader = new InventoryFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_inventory_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(13, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the Quantity field is shifted to the right and should fail to parse, resulting in a null value.            
            Assert.Null((result.DataRows[1] as InventoryDataRow)?.Quantity);
            Assert.Equal(ErrorTypes.MandatoryField, (result.DataRows[1] as InventoryDataRow)?.QuantityError);
        }
    }
}
