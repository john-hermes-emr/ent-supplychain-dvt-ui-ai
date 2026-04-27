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
    public class ItemFileFixture : IDisposable
    {
        public string SampleGoodItemFileContent { get; private set; }
        public string SampleBadItemFileEmptyLinesContent { get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }

        public ItemFileFixture()
        {
            SampleGoodItemFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_item.txt");
            SampleBadItemFileEmptyLinesContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_item_empty_lines.txt");

            SampleBadFileExtraColumnsContent = "Division ID|Local Site ID|Part Number|Description|Comcode|DRI Code|Part Status| Direct_indirect|Purch_mfrd|Lead Time|Standard Cost|Pure_loaded Cost|Currency Code|UOM|ABC Category|Item Weight|Item Weight UOM|Item HTS Code|Item HS Code|" +
                "\r\nD016|VLA-Peterlee|VASTD PAINT (CBA730-DA)|STD EPOXY PAINT (CBA730-DA) AMERON AMERLOCK BETTIS WHITE||T020|A|D|P|16|81.37|L|GBP|EA|C|||||" +
                "\r\nD016|VLA-Peterlee|VASTD PAINT (CBA730-SR)||STD EPOXY PAINT (CBA730-SR) AMERON AMERLOCK BETTIS WHITE||T020|A|D|P|16|84.63|L|GBP|EA|C|||||" +
                "\r\nD016|VLA-Peterlee|VASTD PAINT (CBA830-SR)|STD EPOXY PAINT (CBA830-SR) AMERON AMERLOCK BETTIS WHITE||T020|A|D|P|16|95.48|L|GBP|EA|C|||||";

            SampleBadFileMissingColumnsContent = "Division ID|Local Site ID|Part Number|Description|Comcode|DRI Code|Part Status| Direct_indirect|Purch_mfrd|Lead Time|Standard Cost|Pure_loaded Cost|Currency Code|UOM|ABC Category|Item Weight|Item Weight UOM|Item HTS Code|Item HS Code|" +
                "\r\nD016|VLA-Peterlee|VASTD PAINT (CBA730-DA)|STD EPOXY PAINT (CBA730-DA) AMERON AMERLOCK BETTIS WHITE||T020|A|D|P|16|81.37|L|GBP|EA|C|||||" +
                "\r\nD016|VLA-Peterlee|VASTD PAINT (CBA730-SR)STD EPOXY PAINT (CBA730-SR) AMERON AMERLOCK BETTIS WHITE||T020|A|D|P|16|84.63|L|GBP|EA|C|||||" +
                "\r\nD016|VLA-Peterlee|VASTD PAINT (CBA830-SR)|STD EPOXY PAINT (CBA830-SR) AMERON AMERLOCK BETTIS WHITE||T020|A|D|P|16|95.48|L|GBP|EA|C|||||";
        }
        public void Dispose()
        {
            SampleGoodItemFileContent = string.Empty;
            SampleBadItemFileEmptyLinesContent = string.Empty;
        }
    }

    public class ItemFileLoaderTests : IClassFixture<ItemFileFixture>
    {
        ItemFileFixture _fixture;

        public ItemFileLoaderTests(ItemFileFixture fixture)
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
                .ReturnsAsync(_fixture.SampleGoodItemFileContent);

            ItemFileLoader loader = new ItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_item.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(19, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodItemFileContent);

            ItemFileLoader loader = new ItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_item.txt"
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
                .ReturnsAsync(_fixture.SampleGoodItemFileContent);

            ItemFileLoader loader = new ItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_item.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<ItemDataRow>().ToList();

            //Assert
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber); //First data row
            Assert.Equal("D016", testRow.DivisionId);
            Assert.Equal("VLA-Peterlee", testRow.LocalSiteId);
            Assert.Equal("VASTD PAINT (CBA730-DA)", testRow.PartNumber);
            Assert.Equal("STD EPOXY PAINT (CBA730-DA) AMERON AMERLOCK BETTIS WHITE", testRow.Description);
            Assert.Equal("", testRow.Comcode);
            Assert.Equal("T020", testRow.DRICode);
            Assert.Equal("A", testRow.PartStatus);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("P", testRow.PurchMfrd);
            Assert.NotNull(testRow.LeadTime);
            Assert.Equal(16, testRow.LeadTime.Value);
            Assert.NotNull(testRow.StandardCost);
            Assert.Equal(8137E-2, testRow.StandardCost.Value);
            Assert.Equal("L", testRow.PureLoadedCost);
            Assert.Equal("GBP", testRow.CurrencyCode);
            Assert.Equal("EA", testRow.UOM);
            Assert.Equal("C", testRow.ABCCategory);
            Assert.Null(testRow.ItemWeight);
            Assert.Null(testRow.ItemWeight);
            Assert.Equal("", testRow.ItemWeightUOM);
            Assert.Equal("", testRow.ItemHtsCode);
            Assert.Equal("", testRow.ItemHsCode);

            testRow = dataRows[8];
            Assert.Equal(9, testRow.RowNumber); 
            Assert.Equal("D016", testRow.DivisionId);
            Assert.Equal("VLA-Peterlee", testRow.LocalSiteId);
            Assert.Equal("VASTD PAINT (CBB420-SR)", testRow.PartNumber);
            Assert.Equal("STD EPOXY PAINT (CBB420-SR) AMERON AMERLOCK BETTIS WHITE", testRow.Description);
            Assert.Equal("", testRow.Comcode);
            Assert.Equal("T020", testRow.DRICode);
            Assert.Equal("A", testRow.PartStatus);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("P", testRow.PurchMfrd);
            Assert.NotNull(testRow.LeadTime);
            Assert.Equal(16, testRow.LeadTime.Value);
            Assert.NotNull(testRow.StandardCost);
            Assert.Equal(9765E-2, testRow.StandardCost.Value);
            Assert.Equal("L", testRow.PureLoadedCost);
            Assert.Equal("GBP", testRow.CurrencyCode);
            Assert.Equal("EA", testRow.UOM);
            Assert.Equal("C", testRow.ABCCategory);
            Assert.NotNull(testRow.ItemWeight);
            Assert.Equal(8E-2, testRow.ItemWeight.Value);
            Assert.Equal("KG", testRow.ItemWeightUOM);
            Assert.Equal("90262090", testRow.ItemHtsCode);
            Assert.Equal("902620", testRow.ItemHsCode);
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadItemFileEmptyLinesContent);

            ItemFileLoader loader = new ItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_supplier_empty_lines.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(19, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((ItemDataRow)result.DataRows[2]).DivisionId));
            Assert.True(string.IsNullOrEmpty(((ItemDataRow)result.DataRows[3]).DivisionId));
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

            ItemFileLoader loader = new ItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_item_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(19, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the DateReceived field is shifted to the Left
            Assert.Equal("", (result.DataRows[1] as ItemDataRow)?.Description);
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

            ItemFileLoader loader = new ItemFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_item_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(19, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //In the bad file with extra columns, the Description field is shifted to the right             
            Assert.Equal("", (result.DataRows[1] as ItemDataRow)?.Description);
        }

    }
}
