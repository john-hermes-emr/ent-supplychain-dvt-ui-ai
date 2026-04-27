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
    public class SupplierFileFixture : IDisposable
    {
        public string SampleGoodSupplierFileContent { get; private set; }
        public string SampleBadSupplierFileEmptyLinesContent { get; private set; }
        public string SampleBadFileExtraColumnsContent { get; private set; }
        public string SampleBadFileMissingColumnsContent { get; private set; }

        public SupplierFileFixture()
        {
            SampleGoodSupplierFileContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\good_supplier.txt");
            SampleBadSupplierFileEmptyLinesContent = LoaderTesterHelper.GetFileContent("LoaderTests\\SampleFiles\\bad_supplier_empty_lines.txt");

            //Pipe removed in the suite column on 3rd row
            SampleBadFileMissingColumnsContent = "Division ID|Local Site ID|Supplier ID|Supplier Name|DUNS|Active_inactive|Direct_indirect|Address Descr|Street|Suite|City|State|Postal Code|County|Country|Addr1|Addr2|Addr3|Addr4|Country Code|Global Flag|Main Telephone|Toll Free|Fax|Web site|Supplier Type|" +
                "\r\nD016|VLA-Peterlee|150018786-4787146|APPLETON GROUP LLC||A|D||9377 W HIGGINS RD FL 8||ROSEMONT|IL|60018-4973|COOK|United States|9377 W HIGGINS RD FL 8||||US|U|847-268-6523|0800|||D|" +
                "\r\nD016|VLA-Peterlee|150033794-292861|BIFOLD FLUIDPOWER LTD||A|D||BROADGATE OLDHAM BROADWAY BUSINESS PARKCHADDERTON||OL9 9XA|GREATER MANCHESTER|United Kingdom|BROADGATE OLDHAM BROADWAY BUSINESS PARK||||GB|U|44-161 345 477|0800|44-161 345 477|Bifold.Orders@rotork.com|D|" +
                "\r\nD016|VLA-Peterlee|150036411-1422256|EMERSON PROCESS MANAGEMENT SHARED SERVICES LTD||A|D||BROADGATE OLDHAM BROADWAY BUSINESS PARK||LEICESTER||LE19 1SX|LEICESTERSHIRE|United Kingdom|FOSSE HOUSE 6 SMITH WAY|GROVE PARK ENDERBY|||GB|G|44-01162422400|0800|44-01162422498|UKSales@Emerson.com|D|";

            //Pipe added on street column on 3rd row
            SampleBadFileExtraColumnsContent = "Division ID|Local Site ID|Supplier ID|Supplier Name|DUNS|Active_inactive|Direct_indirect|Address Descr|Street|Suite|City|State|Postal Code|County|Country|Addr1|Addr2|Addr3|Addr4|Country Code|Global Flag|Main Telephone|Toll Free|Fax|Web site|Supplier Type|" +
                "\r\nD016|VLA-Peterlee|150018786-4787146|APPLETON GROUP LLC||A|D||9377 W HIGGINS RD FL 8||ROSEMONT|IL|60018-4973|COOK|United States|9377 W HIGGINS RD FL 8||||US|U|847-268-6523|0800|||D|" +
                "\r\nD016|VLA-Peterlee|150033794-292861|BIFOLD FLUIDPOWER LTD||A|D||BROADGATE OLDHAM BROADWAY BUSINESS| PARK||CHADDERTON||OL9 9XA|GREATER MANCHESTER|United Kingdom|BROADGATE OLDHAM BROADWAY BUSINESS PARK||||GB|U|44-161 345 477|0800|44-161 345 477|Bifold.Orders@rotork.com|D|" +
                "\r\nD016|VLA-Peterlee|150036411-1422256|EMERSON PROCESS MANAGEMENT SHARED SERVICES LTD||A|D||BROADGATE OLDHAM BROADWAY BUSINESS PARK||LEICESTER||LE19 1SX|LEICESTERSHIRE|United Kingdom|FOSSE HOUSE 6 SMITH WAY|GROVE PARK ENDERBY|||GB|G|44-01162422400|0800|44-01162422498|UKSales@Emerson.com|D|";
        }
        public void Dispose()
        {
            SampleGoodSupplierFileContent = string.Empty;
        }
    }

    public class SupplierFileLoaderTests : IClassFixture<SupplierFileFixture>
    {
        SupplierFileFixture _fixture;

        public SupplierFileLoaderTests(SupplierFileFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task FileWithEmptyLines()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleBadSupplierFileEmptyLinesContent);

            SupplierFileLoader loader = new SupplierFileLoader();

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
            Assert.Equal(26, result.FileHeader.Count);
            Assert.True(string.IsNullOrEmpty(((SupplierDataRow)result.DataRows[2]).DivisionId));
            Assert.True(string.IsNullOrEmpty(((SupplierDataRow)result.DataRows[3]).DivisionId));            
        }

        [Fact]
        public async Task GoodFile_ColumnCount()
        {
            //Need to mock the StorageService to return the content of the file from the fixture when GetWorkingFileContentsAsync
            //is called with the correct parameters. Then we can call LoadFileAsync and verify that the FileLoadResult has the expected number of columns in the FileHeader.

            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodSupplierFileContent);

            SupplierFileLoader loader = new SupplierFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_supplier.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(26, result.FileHeader.Count);

            //There should be no rows with incorrect column count in the good file
            Assert.All(result.DataRows, row => Assert.False(row.IncorrectColumnCount));
        }

        [Fact]
        public async Task GoodFile_RowCount()
        {
            //Arrange
            Mock<IStorageService> mockStorageService = new Mock<IStorageService>();
            mockStorageService.Setup(s => s.GetWorkingFileContentsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_fixture.SampleGoodSupplierFileContent);

            SupplierFileLoader loader = new SupplierFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_supplier.txt"
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
                .ReturnsAsync(_fixture.SampleGoodSupplierFileContent);

            SupplierFileLoader loader = new SupplierFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "good_supplier.txt"
            };

            //Act
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);
            var dataRows = result.DataRows.Cast<SupplierDataRow>().ToList();

            //Assert
            Assert.True(result.Success);

            var testRow = dataRows[0];
            Assert.Equal(1, testRow.RowNumber);
            Assert.Equal("D016", testRow.DivisionId);
            Assert.Equal("VLA-Peterlee", testRow.LocalSiteId);
            Assert.Equal("150018786-4787146", testRow.SupplierId);
            Assert.Equal("APPLETON GROUP LLC", testRow.SupplierName);
            Assert.Equal("", testRow.DUNS);
            Assert.Equal("A", testRow.ActiveInactive);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("", testRow.AddressDescr);
            Assert.Equal("9377 W HIGGINS RD FL 8", testRow.Street);
            Assert.Equal("", testRow.Suite);
            Assert.Equal("ROSEMONT", testRow.City);
            Assert.Equal("IL", testRow.State);
            Assert.Equal("60018-4973", testRow.PostalCode);
            Assert.Equal("COOK", testRow.County);
            Assert.Equal("United States", testRow.Country);
            Assert.Equal("9377 W HIGGINS RD FL 8", testRow.Addr1);
            Assert.Equal("", testRow.Addr2);
            Assert.Equal("", testRow.Addr3);
            Assert.Equal("", testRow.Addr4);
            Assert.Equal("US", testRow.CountryCode);
            Assert.Equal("U", testRow.GlobalFlag);
            Assert.Equal("847-268-6523", testRow.MainTelephone);
            Assert.Equal("0800", testRow.TollFree);
            Assert.Equal("", testRow.Fax);
            Assert.Equal("", testRow.WebSite);
            Assert.Equal("D", testRow.SupplierType);

            //Last Row
            testRow = dataRows[8];
            Assert.Equal(9, testRow.RowNumber);
            Assert.Equal("D016", testRow.DivisionId);
            Assert.Equal("VLA-Peterlee", testRow.LocalSiteId);
            Assert.Equal("150059164-160286", testRow.SupplierId);
            Assert.Equal("PROCESS CONTROL EQUIPMENT LTD", testRow.SupplierName);
            Assert.Equal("DUNS1", testRow.DUNS);
            Assert.Equal("A", testRow.ActiveInactive);
            Assert.Equal("D", testRow.DirectIndirect);
            Assert.Equal("Main", testRow.AddressDescr);
            Assert.Equal("TEESSIDE INDUSTRIAL ESTATE DUKESWAY ", testRow.Street);
            Assert.Equal("200", testRow.Suite);
            Assert.Equal("STOCKTON ON TEES", testRow.City);
            Assert.Equal("Bedford", testRow.State);
            Assert.Equal("TS17 9LT", testRow.PostalCode);
            Assert.Equal("DURHAM", testRow.County);
            Assert.Equal("United Kingdom", testRow.Country);
            Assert.Equal("DUKESWAY", testRow.Addr1);
            Assert.Equal("TEESSIDE INDUSTRIAL ESTATE", testRow.Addr2);
            Assert.Equal("Add3", testRow.Addr3);
            Assert.Equal("Addr4", testRow.Addr4);
            Assert.Equal("GB", testRow.CountryCode);
            Assert.Equal("U", testRow.GlobalFlag);
            Assert.Equal("44-01642768250", testRow.MainTelephone);
            Assert.Equal("0800", testRow.TollFree);
            Assert.Equal("44-01642768268", testRow.Fax);
            Assert.Equal("sales@pce-ltd.co.uk", testRow.WebSite);
            Assert.Equal("D", testRow.SupplierType);

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

            SupplierFileLoader loader = new SupplierFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_supplier_missing_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(26, result.FileHeader.Count);

            //We know only the second row has missing columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //Because all these columns are text, they are not marked as invalid.
            //But we're expecting the City to be Manchester but it's shifted so it's set to the postal code
            Assert.Equal("OL9 9XA", (result.DataRows[1] as SupplierDataRow)?.City);
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

            SupplierFileLoader loader = new SupplierFileLoader();

            FileLoadRequest request = new FileLoadRequest
            {
                JobId = Guid.NewGuid(),
                FileName = "bad_supplier_extra_columns.txt"
            };

            //Act            
            var result = await loader.LoadFileAsync(request, mockStorageService.Object);

            //Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FileHeader);
            Assert.Equal(26, result.FileHeader.Count);

            //We know only the second row has extra columns
            Assert.True(result.DataRows[0].IncorrectColumnCount == false);
            Assert.True(result.DataRows[1].IncorrectColumnCount == true);
            Assert.True(result.DataRows[2].IncorrectColumnCount == false);

            //Check a value from the invalid row to make sure it got marked appropriately
            //Because all these columns are text, they are not marked as invalid.
            //But we're expecting the Postal code to be blank since there are extra columns and everything is shifted
            Assert.Equal("",(result.DataRows[1] as SupplierDataRow)?.PostalCode);
        }
    }
}