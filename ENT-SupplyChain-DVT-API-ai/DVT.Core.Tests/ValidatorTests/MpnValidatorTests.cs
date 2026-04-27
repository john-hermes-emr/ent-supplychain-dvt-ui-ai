using DVT.Core.Models.DataRowEntities;
using Xunit;

namespace DVT.Core.Tests.ValidatorTests
{
    public class MPNValidatorTests
    {
        #region Helpers

        private static MPNDataRow NewRow(
            string divisionId = "DIV1",
            string localSiteId = "SITE1",
            string partNumber = "PART001",
            string manufacturerPartNumber = "MPN001",
            string localManufacturerId = "LMID1",
            string manufactureName = "ACME") =>
            new MPNDataRow
            {
                DivisionID = divisionId,
                LocalSiteID = localSiteId,
                PartNumber = partNumber,
                ManufacturerPartNumber = manufacturerPartNumber,
                LocalManufacturerID = localManufacturerId,
                ManufactureName = manufactureName
            };

        #endregion

        #region UniquenessKey - GenerateUniquenessKey

        [Fact]
        public void GenerateUniquenessKey_AllFieldsProvided_SetsExpectedKey()
        {
            // Arrange
            var row = NewRow("DIV1", "SITE1", "PART001", "MPN001", "LMID1", "ACME");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("div1_site1_part001_mpn001_lmid1_acme", row.UniquenessKey);
        }

        [Theory]
        [InlineData(null, "SITE1", "PART001", "MPN001", "LMID1", "ACME")]
        [InlineData("DIV1", null, "PART001", "MPN001", "LMID1", "ACME")]
        [InlineData("DIV1", "SITE1", null, "MPN001", "LMID1", "ACME")]
        [InlineData("DIV1", "SITE1", "PART001", null, "LMID1", "ACME")]
        [InlineData("DIV1", "SITE1", "PART001", "MPN001", null, "ACME")]
        [InlineData("DIV1", "SITE1", "PART001", "MPN001", "LMID1", null)]
        public void GenerateUniquenessKey_NullFields_SetsEmptyKey(
            string divisionId, string localSiteId, string partNumber,
            string manufacturerPartNumber, string localManufacturerId, string manufactureName)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber, manufacturerPartNumber, localManufacturerId, manufactureName);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }

        [Theory]
        [InlineData("   ", "SITE1", "PART001", "MPN001", "LMID1", "ACME")]
        [InlineData("DIV1", "   ", "PART001", "MPN001", "LMID1", "ACME")]
        [InlineData("DIV1", "SITE1", "   ", "MPN001", "LMID1", "ACME")]
        [InlineData("DIV1", "SITE1", "PART001", "   ", "LMID1", "ACME")]
        [InlineData("DIV1", "SITE1", "PART001", "MPN001", "   ", "ACME")]
        [InlineData("DIV1", "SITE1", "PART001", "MPN001", "LMID1", "   ")]
        public void GenerateUniquenessKey_WhitespaceFields_SetsEmptyKey(
            string divisionId, string localSiteId, string partNumber,
            string manufacturerPartNumber, string localManufacturerId, string manufactureName)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, partNumber, manufacturerPartNumber, localManufacturerId, manufactureName);

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
            var row1 = NewRow("DIV1", "SITE1", "PART001", "MPN001", "LMID1", "ACME");
            var row2 = NewRow("DIV1", "SITE1", "PART001", "MPN001", "LMID1", "ACME");

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
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByManufacturerPartNumber_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(manufacturerPartNumber: "MPN001");
            var row2 = NewRow(manufacturerPartNumber: "MPN002");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByLocalManufacturerId_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(localManufacturerId: "LMID1");
            var row2 = NewRow(localManufacturerId: "LMID2");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByManufactureName_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(manufactureName: "ACME");
            var row2 = NewRow(manufactureName: "GLOBEX");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        #endregion
    }
}