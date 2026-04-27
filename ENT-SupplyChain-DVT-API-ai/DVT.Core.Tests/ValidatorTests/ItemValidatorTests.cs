using DVT.Core.Models.DataRowEntities;
using Xunit;

namespace DVT.Core.Tests.ValidatorTests
{
    public class ItemValidatorTests
    {
        #region Helpers

        private static ItemDataRow NewRow(
            string divisionId = "DIV1",
            string localSiteId = "SITE1",
            string partNumber = "PART001") =>
            new ItemDataRow
            {
                DivisionId = divisionId,
                LocalSiteId = localSiteId,
                PartNumber = partNumber
            };

        #endregion

        #region UniquenessKey - GenerateUniquenessKey

        [Fact]
        public void GenerateUniquenessKey_AllFieldsProvided_SetsExpectedKey()
        {
            // Arrange
            var row = NewRow("DIV1", "SITE1", "PART001");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("div1_site1_part001", row.UniquenessKey);
        }

        [Theory]
        [InlineData(null, "SITE1", "PART001")]
        [InlineData("DIV1", null, "PART001")]
        [InlineData("DIV1", "SITE1", null)]
        public void GenerateUniquenessKey_NullFields_SetsEmptyKey(string divisionId, string localSiteId, string partNumber)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }

        [Theory]
        [InlineData("   ", "SITE1", "PART001")]
        [InlineData("DIV1", "   ", "PART001")]
        [InlineData("DIV1", "SITE1", "   ")]
        public void GenerateUniquenessKey_WhitespaceFields_SetsEmptyKey(string divisionId, string localSiteId, string partNumber)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber);

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
            var row1 = NewRow("DIV1", "SITE1", "PART001");
            var row2 = NewRow("DIV1", "SITE1", "PART001");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.Equal(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByDivisionId_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(divisionId: "DIV1");
            var row2 = NewRow(divisionId: "DIV2");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByLocalSiteId_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(localSiteId: "SITE1");
            var row2 = NewRow(localSiteId: "SITE2");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
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

        #endregion
    }
}