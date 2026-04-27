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
    public class PoFileFixture : IDisposable
    {
        public string SampleGoodPoFileContent { get; private set; }
        public string SampleBadPoEmptyLinesFileContent{ get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }

        public PoFileFixture()
        {
            SampleGoodPoFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_po.txt");
            SampleBadPoEmptyLinesFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_po_empty_lines.txt");

            SampleBadFileMissingColumnsContent = "Division ID|Local site ID|PO Number|Order date|Latest Amendment|Commodity Mgr ID|Supplier ID|Currency code|PO Type|Intra-div|Direct_indirect|PO Terms|Freight Terms|EDI|Order Status|Title transfer|Port|" +
                "\r\n0055|VLC-RENO|4500002866|20120622|||2200581|USD|P|N|D|NET60|INCO2010 FOB||O|ORIGIN||" +
                "\r\n0055|VLC-RENO|450007220520140414|||21010|AUD|P|N|D|IMMEDIATE|INCO2010 EXW||O|ORIGIN||" + //Removed pipe before the date column
                "\r\n0055|VLC-RENO|4500075688|20140512|||2200741|USD|P|N|D|NET60|INCO2010 EXW||O|ORIGIN||";

            SampleBadFileExtraColumnsContent = "Division ID|Local site ID|PO Number|Order date|Latest Amendment|Commodity Mgr ID|Supplier ID|Currency code|PO Type|Intra-div|Direct_indirect|PO Terms|Freight Terms|EDI|Order Status|Title transfer|Port|" +
                "\r\n0055|VLC-RENO|4500002866|20120622|||2200581|USD|P|N|D|NET60|INCO2010 FOB||O|ORIGIN||" +
                "\r\n0055|VLC-RENO|4500072205||20140414|||21010|AUD|P|N|D|IMMEDIATE|INCO2010 EXW||O|ORIGIN||" + //Added pipe before the date column
                "\r\n0055|VLC-RENO|4500075688|20140512|||2200741|USD|P|N|D|NET60|INCO2010 EXW||O|ORIGIN||";
        }
        public void Dispose()
        {
            SampleGoodPoFileContent = string.Empty;
            SampleBadPoEmptyLinesFileContent = string.Empty;
        }
    }

    public class PoFileLoaderTests : IClassFixture<PoFileFixture>
    {
        PoFileFixture _fixture;

        public PoFileLoaderTests(PoFileFixture fixture)
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
                .ReturnsAsync(_fixture.SampleGoodPoFileContent);

            POFileLoader loader = new POFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_po.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(17, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodPoFileContent);

            POFileLoader loader = new POFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_po.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.Equal(9, result.DataRows.Count);
        }

        [Fact]
        public async Task GoodFile_VerifyRowData()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodPoFileContent);

            POFileLoader loader = new POFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_po.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<PODataRow>().ToList();

            //Assert
            Assert.True(result.Success);

            //First Row
            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber);
            Assert.Equal("0055", testRow.DivisionID);
            Assert.Equal("VLC-RENO", testRow.LocalSiteID);
            Assert.Equal("4500002866", testRow.PONumber);
            Assert.NotNull(testRow.OrderDate);
            Assert.Equal(new DateTime(2012, 6, 22), testRow.OrderDate.Value);
            Assert.Equal(string.Empty, testRow.OrderDateError);
            Assert.Null(testRow.LatestAmendment);
            Assert.Equal("", testRow.CommodityMGRId);
            Assert.Equal("2200581", testRow.SupplierID);
            Assert.Equal("USD", testRow.CurrencyCode);
            Assert.Equal("P", testRow.POType);
            Assert.Equal("N", testRow.IntraDiv);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("NET60", testRow.POTerms);
            Assert.Equal("INCO2010 FOB", testRow.FreightTerms);
            Assert.Equal("", testRow.EDI);
            Assert.Equal("O", testRow.OrderStatus);
            Assert.Equal("ORIGIN", testRow.TitleTransfer);
            Assert.Equal("", testRow.Port);

            //Last Row
            testRow = dataRows[8];
            Assert.Equal(9, testRow.RowNumber);
            Assert.Equal("0055", testRow.DivisionID);
            Assert.Equal("VLC-RENO", testRow.LocalSiteID);
            Assert.Equal("4500106643", testRow.PONumber);
            Assert.NotNull(testRow.OrderDate);
            Assert.Equal(new DateTime(2015, 1, 19), testRow.OrderDate.Value);
            Assert.Equal(string.Empty, testRow.OrderDateError);
            Assert.NotNull(testRow.LatestAmendment);
            Assert.Equal(new DateTime(2015, 1, 19), testRow.LatestAmendment.Value);
            Assert.Equal(string.Empty, testRow.LatestAmendmentError);
            Assert.Equal("123", testRow.CommodityMGRId);
            Assert.Equal("2200525", testRow.SupplierID);
            Assert.Equal("USD", testRow.CurrencyCode);
            Assert.Equal("P", testRow.POType);
            Assert.Equal("N", testRow.IntraDiv);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("NET30", testRow.POTerms);
            Assert.Equal("INCO2010 FOB", testRow.FreightTerms);
            Assert.Equal("N", testRow.EDI);
            Assert.Equal("O", testRow.OrderStatus);
            Assert.Equal("ORIGIN", testRow.TitleTransfer);
            Assert.Equal("Port-au-prince", testRow.Port);
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadPoEmptyLinesFileContent);

            POFileLoader loader = new POFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_po_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(17, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((PODataRow)result.DataRows[2]).DivisionID));
            Assert.True(string.IsNullOrEmpty(((PODataRow)result.DataRows[3]).DivisionID));
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

            POFileLoader loader = new POFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_po_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(17, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the OrderDate field is shifted to the right and should fail to parse, resulting in a null value.            
            Assert.Null((result.DataRows[1] as PODataRow)?.OrderDate);
            Assert.Equal(Constants.ValidationMessages.MandatoryField, (result.DataRows[1] as PODataRow)?.OrderDateError);
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

            POFileLoader loader = new POFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_po_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(17, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the OrderDate field is shifted to the right and should fail to parse, resulting in a null value.            
            Assert.Null((result.DataRows[1] as PODataRow)?.OrderDate);
            Assert.Equal(Constants.ValidationMessages.MandatoryField, (result.DataRows[1] as PODataRow)?.OrderDateError);
        }
    }
}
