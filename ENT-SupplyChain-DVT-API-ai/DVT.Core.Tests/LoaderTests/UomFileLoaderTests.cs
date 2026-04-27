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

namespace DVT.Core.Tests.LoaderTests
{
    public class UomFileFixture : IDisposable
    {
        public string SampleGoodUomFileContent { get; private set; }
        public string SampleBadUomFileEmptyLinesContent { get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }


        public UomFileFixture()
        {
            SampleGoodUomFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_uom.txt");
            SampleBadUomFileEmptyLinesContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_uom_empty_lines.txt");
            
            SampleBadFileExtraColumnsContent = "Division ID|Local Site ID|Part Number|Local UOM|Base UOM|Conversion Rate|" +
                "\r\nVID1|VIDEOTON-1|C55637-0001|M|EA|1|" +
                "\r\nVID1|VIDEOTON-1|C55588-0001|M|FT|inch|3.2808|" + //Added a column after UOM "inch"
                "\r\nVID1|VIDEOTON-1|C55588-0002|M|FT|3.2808|\r\n";
            
            SampleBadFileMissingColumnsContent = "Division ID|Local Site ID|Part Number|Local UOM|Base UOM|Conversion Rate|" +
                "\r\nVID1|VIDEOTON-1|C55637-0001|M|EA|1|" +
                "\r\nVID1|VIDEOTON-1|C55588-0001MFT|3.2808|" + //Missing the Local UOM and Base UOM columns, and the Part Number column has been concatenated with Local UOM and Base UOM
                "\r\nVID1|VIDEOTON-1|C55588-0002|M|FT|3.2808|\r\n";
        }
        public void Dispose()
        {
            SampleGoodUomFileContent = string.Empty;
            SampleBadUomFileEmptyLinesContent = string.Empty;
        }
    }

    public class UomFileLoaderTests : IClassFixture<UomFileFixture>
    {
        UomFileFixture _fixture;

        public UomFileLoaderTests(UomFileFixture fixture)
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
                .ReturnsAsync(_fixture.SampleGoodUomFileContent);

            UOMFileLoader loader = new UOMFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_uom.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Asset
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(6, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodUomFileContent);

            UOMFileLoader loader = new UOMFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_uom.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Asset
            Assert.True(result.Success);
            Assert.Equal(9, result.DataRows.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }        

        [Fact]
        public async Task GoodFile_VerifyRowData()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodUomFileContent);

            UOMFileLoader loader = new UOMFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_uom.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<UOMDataRow>().ToList();

            //Asset
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber);
            Assert.Equal("VIDEOTON-1", testRow.LocalSiteID);
            Assert.Equal("VID1", testRow.DivisionID);
            Assert.Equal("C55637-0001", testRow.PartNumber);
            Assert.Equal("M", testRow.LocalUOM);
            Assert.Equal("EA", testRow.BaseUOM);
            Assert.Equal(1, testRow.ConversionRate.HasValue ? testRow.ConversionRate.Value : 1);

            testRow = dataRows[1];
            Assert.Equal(2, testRow.RowNumber);
            Assert.Equal("VIDEOTON-1", testRow.LocalSiteID);
            Assert.Equal("VID1", testRow.DivisionID);
            Assert.Equal("C55588-0001", testRow.PartNumber);
            Assert.Equal("M", testRow.LocalUOM);
            Assert.Equal("FT", testRow.BaseUOM);
            Assert.Equal("32808E-4", testRow.ConversionRate.HasValue ? testRow.ConversionRate.Value.ToString() : "");

        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadUomFileEmptyLinesContent);

            UOMFileLoader loader = new UOMFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_uom_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(6, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((UOMDataRow)result.DataRows[2]).DivisionID));
            Assert.True(string.IsNullOrEmpty(((UOMDataRow)result.DataRows[3]).DivisionID));
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

            UOMFileLoader loader = new UOMFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_uom_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(6, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            Assert.Null((result.DataRows[1] as UOMDataRow)?.ConversionRate);
            Assert.Equal("MandatoryField", (result.DataRows[1] as UOMDataRow)?.ConversionRateError.ToString());
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

            UOMFileLoader loader = new UOMFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_uom_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(6, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            Assert.Null((result.DataRows[1] as UOMDataRow)?.ConversionRate);
            Assert.Equal("InvalidFormat", (result.DataRows[1] as UOMDataRow)?.ConversionRateError.ToString());
        }
    }
}
