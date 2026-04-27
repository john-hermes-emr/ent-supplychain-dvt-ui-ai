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
    public class VirFileFixture : IDisposable
    {
        public string SampleGoodVirFileContent { get; private set; }
        public string SampleBadVirFileEmptyLinesContent { get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }


        public VirFileFixture()
        {
            SampleGoodVirFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_vir.txt");
            SampleBadVirFileEmptyLinesContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_vir_empty_lines.txt");

            SampleBadFileExtraColumnsContent = "Division ID|Local Site ID|Receipt Number|PO Number|PO Line Number|Supplier ID|Part Number|Supplier Part Number|Quantity Ordered|Quantity Received|Date Received|Invoice Price Paid|Unit Price|Pure_Loaded Cost|Currency Code|Intra-div|Direct_indirect|PO Terms|Freight Terms|UOM|Title Transfer|Port|Release#|Committed Date|" +
               "\r\nD016|VLA-Peterlee|2681|4293002136|2|150059905-160216|VA-ED-007-9885||3|3|20250226|303.27|101.09|L|GBP|N|D|1ST 2ND PROX|EMR2006|EA|Destination|||20250304|" +
               "\r\nD016|VLA-Peterlee|2692|4293002237|1|150033794-292861|VA000||-501-07||2|2|20250227|81.16|40.58|L|GBP|N|D|5TH 3RD PROX|INCO2010 CPT|EA|Origin|||20250228|" + //7th col has extra pipes
               "\r\nD016|VLA-Peterlee|2692|4293002237|2|150033794-292861|VA000-508-89||1|1|20250227|62.37|62.37|L|GBP|N|D|5TH 3RD PROX|INCO2010 CPT|EA|Origin|||20250228|\r\n";

            SampleBadFileMissingColumnsContent = "Division ID|Local Site ID|Receipt Number|PO Number|PO Line Number|Supplier ID|Part Number|Supplier Part Number|Quantity Ordered|Quantity Received|Date Received|Invoice Price Paid|Unit Price|Pure_Loaded Cost|Currency Code|Intra-div|Direct_indirect|PO Terms|Freight Terms|UOM|Title Transfer|Port|Release#|Committed Date|" +
               "\r\nD016|VLA-Peterlee|2681|4293002136|2|150059905-160216|VA-ED-007-9885||3|3|20250226|303.27|101.09|L|GBP|N|D|1ST 2ND PROX|EMR2006|EA|Destination|||20250304|" +
               "\r\nD016|VLA-Peterlee|2692|4293002237|1|150033794-292861|VA000-501-072|2|20250227|81.16|40.58|L|GBP|N|D|5TH 3RD PROX|INCO2010 CPT|EA|Origin|||20250228|" + //8th column removed pipes
               "\r\nD016|VLA-Peterlee|2692|4293002237|2|150033794-292861|VA000-508-89||1|1|20250227|62.37|62.37|L|GBP|N|D|5TH 3RD PROX|INCO2010 CPT|EA|Origin|||20250228|\r\n";
        }
        public void Dispose()
        {
            SampleGoodVirFileContent = string.Empty;
            SampleBadVirFileEmptyLinesContent = string.Empty;
        }
    }

    public class VirFileLoaderTests : IClassFixture<VirFileFixture>
    {
        VirFileFixture _fixture;

        public VirFileLoaderTests(VirFileFixture fixture)
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
                .ReturnsAsync(_fixture.SampleGoodVirFileContent);

            VirFileLoader loader = new VirFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_vir.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(24, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));           
        }        

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodVirFileContent);

            VirFileLoader loader = new VirFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_vir.txt"
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
                .ReturnsAsync(_fixture.SampleGoodVirFileContent);

            VirFileLoader loader = new VirFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_vir.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<VirDataRow>().ToList();

            //Assert
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber); 
            Assert.Equal("D016", testRow.DivisionId);
            Assert.Equal("VLA-Peterlee", testRow.LocalSiteId);
            Assert.Equal("2681", testRow.ReceiptNumber);
            Assert.Equal("4293002136", testRow.PoNumber);
            Assert.Equal("2", testRow.POLineNumber);
            Assert.Equal("150059905-160216", testRow.SupplierId);
            Assert.Equal("VA-ED-007-9885", testRow.PartNumber);
            Assert.Equal("", testRow.SupplierPartNumber);
            Assert.NotNull(testRow.QuantityOrdered);
            Assert.Equal(3, testRow.QuantityOrdered.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityOrderedError);
            Assert.NotNull(testRow.QuantityReceived);
            Assert.Equal(3, testRow.QuantityReceived.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityReceivedError);
            Assert.NotNull(testRow.DateReceived);
            Assert.Equal(new DateTime(2025, 2, 26), testRow.DateReceived.Value);
            Assert.Equal(string.Empty, testRow.DateReceivedError);
            Assert.NotNull(testRow.InvoicePricePaid);
            Assert.Equal(30327E-2, testRow.InvoicePricePaid.Value);
            Assert.Equal(ErrorTypes.None, testRow.InvoicePricePaidError);
            Assert.NotNull(testRow.UnitPrice);
            Assert.Equal(10109E-2, testRow.UnitPrice.Value);
            Assert.Equal(ErrorTypes.None, testRow.UnitPriceError);
            Assert.Equal("L", testRow.PureLoadedCost);
            Assert.Equal("GBP", testRow.CurrencyCode);
            Assert.Equal("N", testRow.IntraDiv);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("1ST 2ND PROX", testRow.POTerms);
            Assert.Equal("EMR2006", testRow.FreightTerms);
            Assert.Equal("EA", testRow.UOM);
            Assert.Equal("Destination", testRow.TitleTransfer);
            Assert.Equal("", testRow.Port);
            Assert.NotNull(testRow.CommittedDate);
            Assert.Equal(new DateTime(2025, 3, 4), testRow.CommittedDate.Value);
            Assert.Equal(string.Empty, testRow.CommittedDateError);

            //Last Row
            testRow = dataRows[8];
            Assert.Equal(9, testRow.RowNumber);
            Assert.Equal("D016", testRow.DivisionId);
            Assert.Equal("VLA-Peterlee", testRow.LocalSiteId);
            Assert.Equal("2676", testRow.ReceiptNumber);
            Assert.Equal("4293002153", testRow.PoNumber);
            Assert.Equal("1", testRow.POLineNumber);
            Assert.Equal("150060049-160376", testRow.SupplierId);
            Assert.Equal("VAFC2-N CBB525-SR RAL9010", testRow.PartNumber);
            Assert.Equal("123", testRow.SupplierPartNumber);
            Assert.NotNull(testRow.QuantityOrdered);
            Assert.Equal(2, testRow.QuantityOrdered.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityOrderedError);
            Assert.NotNull(testRow.QuantityReceived);
            Assert.Equal(2, testRow.QuantityReceived.Value);
            Assert.Equal(ErrorTypes.None, testRow.QuantityReceivedError);
            Assert.NotNull(testRow.DateReceived);
            Assert.Equal(new DateTime(2025, 2, 25), testRow.DateReceived.Value);
            Assert.Equal(string.Empty, testRow.DateReceivedError);
            Assert.NotNull(testRow.InvoicePricePaid);
            Assert.Equal(180, testRow.InvoicePricePaid.Value);
            Assert.Equal(ErrorTypes.None, testRow.InvoicePricePaidError);
            Assert.NotNull(testRow.UnitPrice);
            Assert.Equal(90, testRow.UnitPrice.Value);
            Assert.Equal(ErrorTypes.None, testRow.UnitPriceError);
            Assert.Equal("L", testRow.PureLoadedCost);
            Assert.Equal("GBP", testRow.CurrencyCode);
            Assert.Equal("N", testRow.IntraDiv);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("1ST 3RD PROX", testRow.POTerms);
            Assert.Equal("EMR2006", testRow.FreightTerms);
            Assert.Equal("EA", testRow.UOM);
            Assert.Equal("Destination", testRow.TitleTransfer);
            Assert.Equal("Port1", testRow.Port);
            Assert.NotNull(testRow.Release);
            Assert.Equal(1, testRow.Release.Value);
            Assert.Equal(ErrorTypes.None, testRow.ReleaseError);
            Assert.NotNull(testRow.CommittedDate);
            Assert.Equal(new DateTime(2025, 3, 3), testRow.CommittedDate.Value);
            Assert.Equal(string.Empty, testRow.CommittedDateError);
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadVirFileEmptyLinesContent);

            VirFileLoader loader = new VirFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_vir_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(24, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((VirDataRow)result.DataRows[2]).DivisionId));
            Assert.True(string.IsNullOrEmpty(((VirDataRow)result.DataRows[3]).DivisionId));
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

            VirFileLoader loader = new VirFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_vir_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(24, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the DateReceived field is shifted to the right and should fail to parse, resulting in a null value.            
            Assert.Null((result.DataRows[1] as VirDataRow)?.DateReceived);
            Assert.Equal("INVALID FORMAT", (result.DataRows[1] as VirDataRow)?.DateReceivedError);
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

            VirFileLoader loader = new VirFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_vir_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(24, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the DateReceived field is shifted to the right and should fail to parse, resulting in a null value.            
            Assert.Null((result.DataRows[1] as VirDataRow)?.DateReceived);
            Assert.Equal("INVALID FORMAT", (result.DataRows[1] as VirDataRow)?.DateReceivedError);
        }
    }
}