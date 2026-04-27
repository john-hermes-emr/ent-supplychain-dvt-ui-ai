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
    public class MpnFileFixture : IDisposable
    {
        public string SampleGoodMpnFileContent { get; private set; }
        public string SampleBadMpnFileEmptyLinesContent { get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }

        public MpnFileFixture()
        {
            SampleGoodMpnFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_mpn.txt");
            SampleBadMpnFileEmptyLinesContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_mpn_empty_lines.txt");

            SampleBadFileExtraColumnsContent = "Division ID|Local Site ID|Part Number|Local Manufacturer ID|Manufacture ID|Manufacture Name|Manufacturer Part Number|Object ID|MPN Type|" +
                "\r\n0016|NI-PEN|100013A-01|49337|1724|QORVO|TGA2526|4CCD851E3F8FF78947BD269D07AB573031D4ADD1|P|" +
                "\r\n0016|NI-DEB|100013A-01||6366238|1724|QORVO|TGA2526|2E1D85B25285701F6E280FE537A4187A832D2C7F|P|" + //Added a pipe after Part Number Column
                "\r\n0016|NI-PEN|100017C-01|4168229|1627|APPLIED THIN-FILM PRODUCTS|ATP-F-010-050-050|99BD36927B191BAA7DA8DE27E445B8E3497AB160|S|";

            SampleBadFileMissingColumnsContent = "Division ID|Local Site ID|Part Number|Local Manufacturer ID|Manufacture ID|Manufacture Name|Manufacturer Part Number|Object ID|MPN Type|" +
                "\r\n0016|NI-PEN|100013A-01|49337|1724|QORVO|TGA2526|4CCD851E3F8FF78947BD269D07AB573031D4ADD1|P|" +
                "\r\n0016|NI-DEB|100013A-016366238|1724|QORVO|TGA2526|2E1D85B25285701F6E280FE537A4187A832D2C7F|P|" + //Removed a pipe between the Local site id and part number
                "\r\n0016|NI-PEN|100017C-01|4168229|1627|APPLIED THIN-FILM PRODUCTS|ATP-F-010-050-050|99BD36927B191BAA7DA8DE27E445B8E3497AB160|S|";
        }
        public void Dispose()
        {
            SampleGoodMpnFileContent = string.Empty;
            SampleBadMpnFileEmptyLinesContent = string.Empty;
        }
    }

    public class MpnFileLoaderTests : IClassFixture<MpnFileFixture>
    {
        MpnFileFixture _fixture;

        public MpnFileLoaderTests(MpnFileFixture fixture)
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
                .ReturnsAsync(_fixture.SampleGoodMpnFileContent);

            MPNFileLoader loader = new MPNFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_mpn.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Asset
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(9, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodMpnFileContent);

            MPNFileLoader loader = new MPNFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_mpn.txt"
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
                .ReturnsAsync(_fixture.SampleGoodMpnFileContent);

            MPNFileLoader loader = new MPNFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_mpn.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<MPNDataRow>().ToList();

            //Asset
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber);
            Assert.Equal("0016", testRow.DivisionID);
            Assert.Equal("NI-PEN", testRow.LocalSiteID);
            Assert.Equal("100013A-01", testRow.PartNumber);
            Assert.Equal("49337", testRow.LocalManufacturerID);
            Assert.Equal("1724", testRow.ManufactureID);
            Assert.Equal("QORVO", testRow.ManufactureName);
            Assert.Equal("TGA2526", testRow.ManufacturerPartNumber);
            Assert.Equal("4CCD851E3F8FF78947BD269D07AB573031D4ADD1", testRow.ObjectID);
            Assert.Equal("P", testRow.MPNType);

            testRow = dataRows[3];
            Assert.Equal(4, testRow.RowNumber);
            Assert.Equal("0016", testRow.DivisionID);
            Assert.Equal("NI-PEN", testRow.LocalSiteID);
            Assert.Equal("100024B-01", testRow.PartNumber);
            Assert.Equal("4168229", testRow.LocalManufacturerID);
            Assert.Equal("1627", testRow.ManufactureID);
            Assert.Equal("APPLIED THIN-FILM PRODUCTS", testRow.ManufactureName);
            Assert.Equal("100024B-01", testRow.ManufacturerPartNumber);
            Assert.Equal("94ABF1C5A6C6B5BA4DED8E838D7473D2ED0F71C2", testRow.ObjectID);
            Assert.Equal("P", testRow.MPNType);
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadMpnFileEmptyLinesContent);

            MPNFileLoader loader = new MPNFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_mpn_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(9, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((MPNDataRow)result.DataRows[2]).DivisionID));
            Assert.True(string.IsNullOrEmpty(((MPNDataRow)result.DataRows[3]).DivisionID));
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

            MPNFileLoader loader = new MPNFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_mpn_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(9, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with missing columns, the LocalManufacturerId field is shifted to the left
            //The value should be 6366238 but instead it's 1724
            Assert.Equal("1724", (result.DataRows[1] as MPNDataRow)?.LocalManufacturerID);            
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

            MPNFileLoader loader = new MPNFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_mpn_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(9, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the LocalManufacturerId field is shifted to the right
            //The value should be 6366238 but instead it's blank because the new column is blank
            Assert.Equal("", (result.DataRows[1] as MPNDataRow)?.LocalManufacturerID);
        }
    }
}