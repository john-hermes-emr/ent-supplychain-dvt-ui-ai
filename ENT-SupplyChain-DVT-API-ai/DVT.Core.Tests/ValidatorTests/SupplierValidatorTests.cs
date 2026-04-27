using DVT.Core.Models.DataRowEntities;
using Xunit;

namespace DVT.Core.Tests.ValidatorTests
{
    public class SupplierValidatorTests
    {
        #region Helpers

        private static SupplierDataRow NewRow(
            string divisionId = "DIV1",
            string localSiteId = "SITE1",
            string supplierId = "SUP001") =>
            new SupplierDataRow
            {
                DivisionId = divisionId,
                LocalSiteId = localSiteId,
                SupplierId = supplierId
            };

        #endregion

        #region UniquenessKey - GenerateUniquenessKey

        [Fact]
        public void GenerateUniquenessKey_AllFieldsProvided_SetsExpectedKey()
        {
            // Arrange
            var row = NewRow("DIV1", "SITE1", "SUP001");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("div1_site1_sup001", row.UniquenessKey);
        }

        [Theory]
        [InlineData(null, "SITE1", "SUP001")]
        [InlineData("DIV1", null, "SUP001")]
        [InlineData("DIV1", "SITE1", null)]
        public void GenerateUniquenessKey_NullFields_SetsEmptyKey(string divisionId, string localSiteId, string supplierId)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, supplierId);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }

        [Theory]
        [InlineData("   ", "SITE1", "SUP001")]
        [InlineData("DIV1", "   ", "SUP001")]
        [InlineData("DIV1", "SITE1", "   ")]
        public void GenerateUniquenessKey_WhitespaceFields_SetsEmptyKey(string divisionId, string localSiteId, string supplierId)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, supplierId);

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
            var row1 = NewRow("DIV1", "SITE1", "SUP001");
            var row2 = NewRow("DIV1", "SITE1", "SUP001");

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
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyBySupplierId_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(supplierId: "SUP001");
            var row2 = NewRow(supplierId: "SUP002");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        #endregion
    }
}