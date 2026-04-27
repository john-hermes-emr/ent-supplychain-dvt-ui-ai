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
    public class PoItemFileFixture : IDisposable
    {
        public string SampleGoodPoItemFileContent { get; private set; }
        public string SampleBadPoItemFileEmptyLinesContent { get; private set; }
        public string SampleBadPoItemExtraColumnsContent { get; private set; }
        public string SampleBadPoItemMissingColumnsContent { get; private set; }

        public PoItemFileFixture()
        {
            SampleGoodPoItemFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_poitem.txt");
            SampleBadPoItemFileEmptyLinesContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_poitem_empty_lines.txt");

            SampleBadPoItemExtraColumnsContent = "Division ID|Local site ID|PO Number|PO line Number|Part number|Supplier part number|Description|Contract ID|Unit cost|Pure_loaded cost|Ordered value|Quantity ordered|Quantity returned|Committed date|Requested date|Order status|Currency code|UOM|Qty left to receive|Value left to receive|Release#|" +
                "\r\n0055|VLC-RENO|4500117704|00010|11023171||ASY STM PCS17-16 304||134.0000|P|536.00|4.000||20150619||O|USD|EA|8.000|1072.00||" +
                "\r\n0055|VLC-RENO|4500137293|00010|11021388||CYL AIR |test|KGD18-AC14 RDC VC||1092.9100|P|2185.82|2.000||20151001||O|USD|EA|4.000|4371.64||" +
                "\r\n0055|VLC-RENO|4500218724|00240|11012949||STU 1-8 X6.25 PLTD A193-B7||4.2800|P|42.80|10.000||20160909||O|USD|EA|20.000|85.60||";

            SampleBadPoItemMissingColumnsContent = "Division ID|Local site ID|PO Number|PO line Number|Part number|Supplier part number|Description|Contract ID|Unit cost|Pure_loaded cost|Ordered value|Quantity ordered|Quantity returned|Committed date|Requested date|Order status|Currency code|UOM|Qty left to receive|Value left to receive|Release#|" +
                "\r\n0055|VLC-RENO|4500117704|00010|11023171||ASY STM PCS17-16 304||134.0000|P|536.00|4.000||20150619||O|USD|EA|8.000|1072.00||" +
                "\r\n0055|VLC-RENO|4500137293|00010|11021388||CYL AIR KGD18-AC14 RDC VC1092.9100|P|2185.82|2.000||20151001||O|USD|EA|4.000|4371.64||" +
                "\r\n0055|VLC-RENO|4500218724|00240|11012949||STU 1-8 X6.25 PLTD A193-B7||4.2800|P|42.80|10.000||20160909||O|USD|EA|20.000|85.60||";
        }
        public void Dispose()
        {
            SampleGoodPoItemFileContent = string.Empty;
            SampleBadPoItemFileEmptyLinesContent = string.Empty;
        }
    }

    public class PoItemFileLoaderTests : IClassFixture<PoItemFileFixture>
    {
        PoItemFileFixture _fixture;

        public PoItemFileLoaderTests(PoItemFileFixture fixture)
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
                .ReturnsAsync(_fixture.SampleGoodPoItemFileContent);

            POItemFileLoader loader = new POItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_poitem.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(21, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }        

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodPoItemFileContent);

            POItemFileLoader loader = new POItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_poitem.txt"
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
                .ReturnsAsync(_fixture.SampleGoodPoItemFileContent);

            POItemFileLoader loader = new POItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_poitem.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<POItemDataRow>().ToList();

            //Assert
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber);
            Assert.Equal("0055", testRow.DivisionID);
            Assert.Equal("VLC-RENO", testRow.LocalSiteID);
            Assert.Equal("4500117704", testRow.PONumber);
            Assert.Equal("00010", testRow.POLineNumber);
            Assert.Equal("11023171", testRow.PartNumber);
            Assert.Equal("", testRow.SupplierPartNumber);
            Assert.Equal("ASY STM PCS17-16 304", testRow.Description);
            Assert.Equal("", testRow.ContractID);
            Assert.NotNull(testRow.UnitCost);
            Assert.Equal(134, testRow.UnitCost.Value);
            Assert.Equal(ErrorTypes.None, testRow.UnitCostError);
            Assert.Equal("P", testRow.PureLoadedCost);
            Assert.NotNull(testRow.OrderedValue);
            Assert.Equal(536, testRow.OrderedValue.Value);
            Assert.Equal(ErrorTypes.None, testRow.OrderedValueError);
            Assert.NotNull(testRow.QuantityOrdered);
            Assert.Equal(4, testRow.QuantityOrdered.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityOrderedError);            
            Assert.Null(testRow.QuantityReturned);
            Assert.NotNull(testRow.CommittedDate);
            Assert.Equal(new DateTime(2015, 6, 19), testRow.CommittedDate.Value);
            Assert.Empty(testRow.CommittedDateError);            
            Assert.NotEmpty(testRow.RequestedDateError);
            Assert.Equal("O", testRow.OrderStatus);
            Assert.Equal("USD", testRow.CurrencyCode);
            Assert.Equal("EA", testRow.UOM);
            Assert.NotNull(testRow.QtyLeftToReceive);
            Assert.Equal(8, testRow.QtyLeftToReceive.Value);
            Assert.Equal(ErrorTypes.None, testRow.QtyLeftToReceiveError);
            Assert.NotNull(testRow.ValueLeftToReceive);
            Assert.Equal(1072, testRow.ValueLeftToReceive.Value);
            Assert.Equal(ErrorTypes.None, testRow.ValueLeftToReceiveError);            
            Assert.Null(testRow.Release);
            Assert.Equal(ErrorTypes.MandatoryField, testRow.ReleaseError); //We know the Release error field is optional and it's empty in the file, so it should be set to the error message

            //Last Row
            testRow = dataRows[8];
            Assert.Equal(9, testRow.RowNumber);
            Assert.Equal("0055", testRow.DivisionID);
            Assert.Equal("VLC-RENO", testRow.LocalSiteID);
            Assert.Equal("4500634157", testRow.PONumber);
            Assert.Equal("00030", testRow.POLineNumber);
            Assert.Equal("11576920", testRow.PartNumber);
            Assert.Equal("123", testRow.SupplierPartNumber);
            Assert.Equal("SEA KS1 TRANSVERSE FKM ZC-770", testRow.Description);
            Assert.Equal("123", testRow.ContractID);
            Assert.NotNull(testRow.UnitCost);
            Assert.Equal(3942E-2, testRow.UnitCost.Value);
            Assert.Equal(ErrorTypes.None, testRow.UnitCostError);
            Assert.Equal("P", testRow.PureLoadedCost);
            Assert.NotNull(testRow.OrderedValue);
            Assert.Equal(236520E-2, testRow.OrderedValue.Value);
            Assert.Equal(ErrorTypes.None, testRow.OrderedValueError);
            Assert.NotNull(testRow.QuantityOrdered);
            Assert.Equal(60, testRow.QuantityOrdered.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityOrderedError);
            Assert.NotNull(testRow.QuantityReturned);
            Assert.Equal(1, testRow.QuantityReturned.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityReturnedError);
            Assert.NotNull(testRow.CommittedDate);
            Assert.Equal(new DateTime(2025, 6, 30), testRow.CommittedDate.Value);
            Assert.Equal(string.Empty, testRow.CommittedDateError);
            Assert.NotNull(testRow.RequestedDate);
            Assert.Equal(new DateTime(2025, 6, 30), testRow.RequestedDate.Value);
            Assert.Equal(string.Empty, testRow.RequestedDateError);
            Assert.Equal("O", testRow.OrderStatus);
            Assert.Equal("USD", testRow.CurrencyCode);
            Assert.Equal("FT", testRow.UOM);
            Assert.NotNull(testRow.QtyLeftToReceive);
            Assert.Equal(48, testRow.QtyLeftToReceive.Value);
            Assert.Equal(ErrorTypes.None, testRow.QtyLeftToReceiveError);
            Assert.NotNull(testRow.ValueLeftToReceive);
            Assert.Equal(189216E-2, testRow.ValueLeftToReceive.Value);
            Assert.Equal(ErrorTypes.None, testRow.ValueLeftToReceiveError);
            Assert.NotNull(testRow.Release);
            Assert.Equal(1, testRow.Release.Value);
            Assert.Equal(ErrorTypes.None, testRow.ReleaseError);
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadPoItemFileEmptyLinesContent);

            POItemFileLoader loader = new POItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_poitem_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(21, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((POItemDataRow)result.DataRows[2]).DivisionID));
            Assert.True(string.IsNullOrEmpty(((POItemDataRow)result.DataRows[3]).DivisionID));
        }

        [Fact]
        public async Task BadFile_MissingColumns()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadPoItemMissingColumnsContent);

            POItemFileLoader loader = new POItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_poitem_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(21, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            Assert.Null((result.DataRows[1] as POItemDataRow)?.CommittedDate);
            Assert.Equal("INVALID FORMAT", (result.DataRows[1] as POItemDataRow)?.CommittedDateError);
        }

        [Fact]
        public async Task BadFile_ExtraColumns()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadPoItemMissingColumnsContent);

            POItemFileLoader loader = new POItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_poitem_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(21, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            Assert.Null((result.DataRows[1] as POItemDataRow)?.CommittedDate);
            Assert.Equal("INVALID FORMAT", (result.DataRows[1] as POItemDataRow)?.CommittedDateError);
        }
    }
}