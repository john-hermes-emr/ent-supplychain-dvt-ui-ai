using DVT.Core.Models.DataRowEntities;
using Xunit;

namespace DVT.Core.Tests.ValidatorTests
{
    public class UOMValidatorTests
    {
        #region Helpers

        private static UOMDataRow NewRow(
            string divisionId = "DIV1",
            string localSiteId = "SITE1",
            string partNumber = "PART001",
            string localUOM = "EA",
            string baseUOM = "KG") =>
            new UOMDataRow
            {
                DivisionID = divisionId,
                LocalSiteID = localSiteId,
                PartNumber = partNumber,
                LocalUOM = localUOM,
                BaseUOM = baseUOM
            };

        #endregion

        #region UniquenessKey - GenerateUniquenessKey

        [Fact]
        public void GenerateUniquenessKey_AllFieldsProvided_SetsExpectedKey()
        {
            // Arrange
            var row = NewRow("DIV1", "SITE1", "PART001", "EA", "KG");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("DIV1_SITE1_PART001_EA_KG", row.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_PreservesOriginalCasing()
        {
            // Arrange
            var row = NewRow("div1", "site1", "part001", "ea", "kg");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("div1_site1_part001_ea_kg", row.UniquenessKey);
        }

        [Theory]
        [InlineData(null, "SITE1", "PART001", "EA", "KG")]
        [InlineData("DIV1", null, "PART001", "EA", "KG")]
        [InlineData("DIV1", "SITE1", null, "EA", "KG")]
        [InlineData("DIV1", "SITE1", "PART001", null, "KG")]
        [InlineData("DIV1", "SITE1", "PART001", "EA", null)]
        public void GenerateUniquenessKey_NullFields_SetsEmptyKey(
            string divisionId, string localSiteId, string partNumber, string localUOM, string baseUOM)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber, localUOM, baseUOM);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }

        [Theory]
        [InlineData("   ", "SITE1", "PART001", "EA", "KG")]
        [InlineData("DIV1", "   ", "PART001", "EA", "KG")]
        [InlineData("DIV1", "SITE1", "   ", "EA", "KG")]
        [InlineData("DIV1", "SITE1", "PART001", "   ", "KG")]
        [InlineData("DIV1", "SITE1", "PART001", "EA", "   ")]
        public void GenerateUniquenessKey_WhitespaceFields_SetsEmptyKey(
            string divisionId, string localSiteId, string partNumber, string localUOM, string baseUOM)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber, localUOM, baseUOM);

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
            var row1 = NewRow("DIV1", "SITE1", "PART001", "EA", "KG");
            var row2 = NewRow("DIV1", "SITE1", "PART001", "EA", "KG");

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
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByLocalUOM_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(localUOM: "EA");
            var row2 = NewRow(localUOM: "LB");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByBaseUOM_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(baseUOM: "KG");
            var row2 = NewRow(baseUOM: "LB");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        #endregion
    }
}