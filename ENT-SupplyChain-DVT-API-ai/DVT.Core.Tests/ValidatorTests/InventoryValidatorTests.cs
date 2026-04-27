using DVT.Core.Helper;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Validators;
using FluentValidation.Results;
using System;
using System.Linq;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ValidatorTests
{
    public class InventoryValidatorTests
    {
        #region Helpers

        private static InventoryDataRow NewRow(
            string divisionId = "DIV1",
            string localSiteId = "SITE1",
            string partNumber = "PART001",
            string inventoryDateStr = "2024-01-15") =>
            new InventoryDataRow
            {
                DivisionId = divisionId,
                LocalSiteId = localSiteId,
                PartNumber = partNumber,
                InventoryDateStr = inventoryDateStr
            };

        #endregion

        #region UniquenessKey - GenerateUniquenessKey

        [Fact]
        public void GenerateUniquenessKey_AllFieldsProvided_SetsExpectedKey()
        {
            // Arrange
            var row = NewRow("DIV1", "SITE1", "PART001", "2024-01-15");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("div1_site1_part001_2024-01-15", row.UniquenessKey);
        }

        [Theory]
        [InlineData(null, "SITE1", "PART001", "2024-01-15")]
        [InlineData("DIV1", null, "PART001", "2024-01-15")]
        [InlineData("DIV1", "SITE1", null, "2024-01-15")]
        [InlineData("DIV1", "SITE1", "PART001", null)]
        public void GenerateUniquenessKey_NullFields_SetsEmptyKey(string divisionId, string localSiteId, string partNumber, string inventoryDateStr)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber, inventoryDateStr);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }

        [Theory]
        [InlineData("   ", "SITE1", "PART001", "2024-01-15")]
        [InlineData("DIV1", "   ", "PART001", "2024-01-15")]
        [InlineData("DIV1", "SITE1", "   ", "2024-01-15")]
        [InlineData("DIV1", "SITE1", "PART001", "   ")]
        public void GenerateUniquenessKey_EmptyStringFields_SetsEmptyKey(string divisionId, string localSiteId, string partNumber, string inventoryDateStr)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber, inventoryDateStr);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }        

        [Fact]
        public void UniquenessKey_BeforeGenerateCalled_IsEmpty()
        {
            // Arrange
            var row = NewRow();

            // Assert - no GenerateUniquenessKey call
            Assert.Equal("", row.UniquenessKey);
        }
        
        [Fact]
        public void GenerateUniquenessKey_TwoRowsWithSameFields_ProduceSameKey()
        {
            // Arrange
            var row1 = NewRow("DIV1", "SITE1", "PART001", "2024-01-15");
            var row2 = NewRow("DIV1", "SITE1", "PART001", "2024-01-15");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.Equal(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByPartNumber_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(partNumber: "PART001");
            var row2 = NewRow(partNumber: "PART002");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByInventoryDate_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(inventoryDateStr: "2024-01-15");
            var row2 = NewRow(inventoryDateStr: "2024-01-16");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        #endregion
    }
}