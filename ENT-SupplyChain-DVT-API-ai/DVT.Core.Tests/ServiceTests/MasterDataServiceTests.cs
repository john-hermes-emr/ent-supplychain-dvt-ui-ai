using DVT.Core;
using DVT.Core.Models;
using DVT.Core.Services;
using FluentValidation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ServiceTests
{
    public class MasterDataServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IValidator<MasterData>> _validatorMock;
        private readonly MasterDataService _service;

        public MasterDataServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _validatorMock = new Mock<IValidator<MasterData>>();
            _service = new MasterDataService(_unitOfWorkMock.Object, _validatorMock.Object);
        }

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_ShouldReturnOrderedDivisions_WhenDivisionsExist()
        {
            // Arrange
            var divisions = new List<MasterData>
            {
                new MasterData
                {
                    ItemId = Guid.NewGuid(),
                    TableName = MasterDataTableNames.Division,
                    TextId = "DIV002",
                    ItemName = "Division B",
                    ItemNameAbbrev = "DIVB"
                },
                new MasterData
                {
                    ItemId = Guid.NewGuid(),
                    TableName = MasterDataTableNames.Division,
                    TextId = "DIV001",
                    ItemName = "Division A",
                    ItemNameAbbrev = "DIVA"
                }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(MasterDataTableNames.Division))
                .ReturnsAsync(divisions);

            // Act
            var result = await _service.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            var orderedResult = result.ToList();
            Assert.Equal("DIV001", orderedResult[0].TextId);
            Assert.Equal("DIV002", orderedResult[1].TextId);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ShouldReturnEmpty_WhenNoDivisionsExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(MasterDataTableNames.Division))
                .ReturnsAsync(new List<MasterData>());

            // Act
            var result = await _service.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(MasterDataTableNames.Division))
                .ReturnsAsync((IEnumerable<MasterData>)null);

            // Act
            var result = await _service.GetAllDivisionsAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ShouldSortByTextIdAndItemName()
        {
            // Arrange
            var divisions = new List<MasterData>
            {
                new MasterData { TextId = "C", ItemName = "Charlie", ItemNameAbbrev = "C" },
                new MasterData { TextId = "A", ItemName = "Alpha", ItemNameAbbrev = "A" },
                new MasterData { TextId = "B", ItemName = "Bravo", ItemNameAbbrev = "B" }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(MasterDataTableNames.Division))
                .ReturnsAsync(divisions);

            // Act
            var result = await _service.GetAllDivisionsAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal("A", resultList[0].TextId);
            Assert.Equal("B", resultList[1].TextId);
            Assert.Equal("C", resultList[2].TextId);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMasterData_WhenIdExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var masterData = new MasterData
            {
                ItemId = id,
                TableName = MasterDataTableNames.Division,
                TextId = "DIV001",
                ItemName = "Division A"
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetByIdAsync(id))
                .ReturnsAsync(masterData);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.ItemId);
            Assert.Equal("DIV001", result.TextId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenIdDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.MasterData.GetByIdAsync(id))
                .ReturnsAsync((MasterData)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetByIdAsync(id));
            Assert.Contains("Item was not found", exception.Message);
        }

        #endregion

        #region GetAllTableNamesAsync Tests

        [Fact]
        public async Task GetAllTableNamesAsync_ShouldReturnOrderedTableNames_WhenTablesExist()
        {
            // Arrange
            var tableNames = new List<string> { "UOM", "Division", "Currency", "SiteMaster" };

            _unitOfWorkMock.Setup(x => x.MasterData.GetAllTableNamesAsync())
                .ReturnsAsync(tableNames);

            // Act
            var result = await _service.GetAllTableNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
            var orderedResult = result.ToList();
            Assert.Equal("Currency", orderedResult[0]);
            Assert.Equal("Division", orderedResult[1]);
            Assert.Equal("SiteMaster", orderedResult[2]);
            Assert.Equal("UOM", orderedResult[3]);
        }

        [Fact]
        public async Task GetAllTableNamesAsync_ShouldReturnEmpty_WhenNoTablesExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.MasterData.GetAllTableNamesAsync())
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _service.GetAllTableNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllTableNamesAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.MasterData.GetAllTableNamesAsync())
                .ReturnsAsync((IEnumerable<string>)null);

            // Act
            var result = await _service.GetAllTableNamesAsync();

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetMasterDataByTableNamesAsync Tests

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldReturnOrderedData_WhenDataExists()
        {
            // Arrange
            var tableName = MasterDataTableNames.UOM;
            var masterDataList = new List<MasterData>
            {
                new MasterData
                {
                    TableName = tableName,
                    TextId = "EA",
                    ItemName = "Each",
                    ItemNameAbbrev = "EA"
                },
                new MasterData
                {
                    TableName = tableName,
                    TextId = "LB",
                    ItemName = "Pound",
                    ItemNameAbbrev = "LB"
                }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(tableName))
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetMasterDataByTableNamesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldThrowException_WhenTableNameIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetMasterDataByTableNamesAsync(null));
        }

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldThrowException_WhenTableNameIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetMasterDataByTableNamesAsync(string.Empty));
        }

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldThrowException_WhenTableNameIsWhitespace()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetMasterDataByTableNamesAsync("   "));
        }

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldReturnEmpty_WhenNoDataExists()
        {
            // Arrange
            var tableName = "NonExistentTable";
            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(tableName))
                .ReturnsAsync(new List<MasterData>());

            // Act
            var result = await _service.GetMasterDataByTableNamesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldSortByMultipleFields()
        {
            // Arrange
            var tableName = "TestTable";
            var masterDataList = new List<MasterData>
            {
                new MasterData { TableName = tableName, TextId = "C", ItemName = "Charlie", ItemNameAbbrev = "C" },
                new MasterData { TableName = tableName, TextId = "A", ItemName = "Alpha", ItemNameAbbrev = "A" },
                new MasterData { TableName = tableName, TextId = "B", ItemName = "Bravo", ItemNameAbbrev = "B" }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(tableName))
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetMasterDataByTableNamesAsync(tableName);

            // Assert
            var resultList = result.ToList();
            Assert.Equal("A", resultList[0].TextId);
            Assert.Equal("B", resultList[1].TextId);
            Assert.Equal("C", resultList[2].TextId);
        }

        #endregion

        #region GetAllMasterDataAsync Tests

        [Fact]
        public async Task GetAllMasterDataAsync_ShouldReturnOrderedData_WhenDataExists()
        {
            // Arrange
            var masterDataList = new List<MasterData>
            {
                new MasterData
                {
                    ItemId = Guid.NewGuid(),
                    TableName = "ZTable",
                    TextId = "Z001",
                    ItemName = "Zebra",
                    ItemNameAbbrev = "ZBR"
                },
                new MasterData
                {
                    ItemId = Guid.NewGuid(),
                    TableName = "ATable",
                    TextId = "A001",
                    ItemName = "Alpha",
                    ItemNameAbbrev = "ALP"
                }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetAllNoTrackingAsync())
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetAllMasterDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllMasterDataAsync_ShouldReturnEmpty_WhenNoDataExists()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.MasterData.GetAllAsync())
                .ReturnsAsync(new List<MasterData>());

            // Act
            var result = await _service.GetAllMasterDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMasterDataAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.MasterData.GetAllNoTrackingAsync())
                .ReturnsAsync((IEnumerable<MasterData>)null);

            // Act
            var result = await _service.GetAllMasterDataAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllMasterDataAsync_ShouldSortByMultipleFieldsCorrectly()
        {
            // Arrange
            var masterDataList = new List<MasterData>
            {
                new MasterData { TableName = "Table2", TextId = "B", ItemName = "Beta", ItemNameAbbrev = "BET" },
                new MasterData { TableName = "Table1", TextId = "A", ItemName = "Alpha", ItemNameAbbrev = "ALP" },
                new MasterData { TableName = "Table1", TextId = "B", ItemName = "Bravo", ItemNameAbbrev = "BRV" }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetAllNoTrackingAsync())
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetAllMasterDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        #endregion

        #region GetMasterDataByIdAsync Tests

        [Fact]
        public async Task GetMasterDataByIdAsync_ShouldReturnMasterData_WhenIdExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var masterData = new MasterData
            {
                ItemId = id,
                TableName = MasterDataTableNames.Currency,
                TextId = "USD",
                ItemName = "US Dollar"
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetByIdAsync(id))
                .ReturnsAsync(masterData);

            // Act
            var result = await _service.GetMasterDataByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.ItemId);
            Assert.Equal("USD", result.TextId);
        }

        [Fact]
        public async Task GetMasterDataByIdAsync_ShouldThrowException_WhenIdDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.MasterData.GetByIdAsync(id))
                .ReturnsAsync((MasterData)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetMasterDataByIdAsync(id));
            Assert.Contains("Item was not found", exception.Message);
            Assert.Contains(id.ToString(), exception.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task GetAllDivisionsAsync_ShouldHandleMultipleDivisionsWithSameName()
        {
            // Arrange
            var divisions = new List<MasterData>
            {
                new MasterData
                {
                    ItemId = Guid.NewGuid(),
                    TableName = MasterDataTableNames.Division,
                    TextId = "DIV001",
                    ItemName = "Same Name",
                    ItemNameAbbrev = "SAME1"
                },
                new MasterData
                {
                    ItemId = Guid.NewGuid(),
                    TableName = MasterDataTableNames.Division,
                    TextId = "DIV002",
                    ItemName = "Same Name",
                    ItemNameAbbrev = "SAME2"
                }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(MasterDataTableNames.Division))
                .ReturnsAsync(divisions);

            // Act
            var result = await _service.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Theory]
        [InlineData("Division")]
        [InlineData("SiteMaster")]
        [InlineData("UOM")]
        [InlineData("Currency")]
        [InlineData("CommodityCode")]
        [InlineData("FreightTerms")]
        [InlineData("PaymentTerm")]
        public async Task GetMasterDataByTableNamesAsync_ShouldHandleAllKnownTableNames(string tableName)
        {
            // Arrange
            var masterDataList = new List<MasterData>
            {
                new MasterData { TableName = tableName, TextId = "TEST1", ItemName = "Test 1" }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(tableName))
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetMasterDataByTableNamesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(tableName, result.First().TableName);
        }

        [Fact]
        public async Task GetAllMasterDataAsync_ShouldIncludeAllTableTypes()
        {
            // Arrange
            var masterDataList = new List<MasterData>
            {
                new MasterData { TableName = MasterDataTableNames.Division, TextId = "DIV1", ItemName = "Division 1" },
                new MasterData { TableName = MasterDataTableNames.UOM, TextId = "EA", ItemName = "Each" },
                new MasterData { TableName = MasterDataTableNames.Currency, TextId = "USD", ItemName = "US Dollar" }
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetAllNoTrackingAsync())
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetAllMasterDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, x => x.TableName == MasterDataTableNames.Division);
            Assert.Contains(result, x => x.TableName == MasterDataTableNames.UOM);
            Assert.Contains(result, x => x.TableName == MasterDataTableNames.Currency);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetByIdAsync_ShouldVerifyRepositoryCalledOnce()
        {
            // Arrange
            var id = Guid.NewGuid();
            var masterData = new MasterData { ItemId = id, TextId = "TEST" };

            _unitOfWorkMock.Setup(x => x.MasterData.GetByIdAsync(id))
                .ReturnsAsync(masterData);

            // Act
            await _service.GetByIdAsync(id);

            // Assert
            _unitOfWorkMock.Verify(x => x.MasterData.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetAllTableNamesAsync_ShouldHandleDuplicateTableNames()
        {
            // Arrange - Even though duplicates shouldn't exist, test sorting behavior
            var tableNames = new List<string> { "Table1", "Table2", "Table1" };

            _unitOfWorkMock.Setup(x => x.MasterData.GetAllTableNamesAsync())
                .ReturnsAsync(tableNames);

            // Act
            var result = await _service.GetAllTableNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetMasterDataByTableNamesAsync_ShouldHandleCaseInsensitiveTableName()
        {
            // Arrange
            var tableName = "division"; // lowercase
            var masterDataList = new List<MasterData>
            {
                new MasterData { TableName = "Division", TextId = "DIV1" } // Different case
            };

            _unitOfWorkMock.Setup(x => x.MasterData.GetMasterDataByTableNamesAsync(tableName))
                .ReturnsAsync(masterDataList);

            // Act
            var result = await _service.GetMasterDataByTableNamesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        #endregion
    }
}