using DVT.Core.Models.DataRowEntities;
using Xunit;

namespace DVT.Core.Tests.ValidatorTests
{
    public class POItemValidatorTests
    {
        #region Helpers

        private static POItemDataRow NewRow(
            string divisionId = "DIV1",
            string localSiteId = "SITE1",
            string poNumber = "PO001",
            string poLineNumber = "LINE1",
            string committedDateStr = "2024-01-15",
            string requestedDateStr = "2024-02-15",
            string releaseOriginalStr = "REL1") =>
            new POItemDataRow
            {
                DivisionID = divisionId,
                LocalSiteID = localSiteId,
                PONumber = poNumber,
                POLineNumber = poLineNumber,
                CommittedDateStr = committedDateStr,
                RequestedDateStr = requestedDateStr,
                ReleaseOriginalStr = releaseOriginalStr
            };

        #endregion

        #region UniquenessKey - GenerateUniquenessKey

        [Fact]
        public void GenerateUniquenessKey_AllFieldsProvided_SetsExpectedKey()
        {
            // Arrange
            var row = NewRow("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1");

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("div1_site1_po001_line1_2024-01-15_2024-02-15_rel1", row.UniquenessKey);
        }

        [Theory]
        [InlineData(null, "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", null, "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", null, "LINE1", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", null, "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "LINE1", null, "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", null, "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", null)]
        public void GenerateUniquenessKey_NullFields_SetsEmptyKey(
            string divisionId, string localSiteId, string poNumber, string poLineNumber,
            string committedDateStr, string requestedDateStr, string releaseOriginalStr)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, poNumber, poLineNumber, committedDateStr, requestedDateStr, releaseOriginalStr);

            // Act
            row.GenerateUniquenessKey();

            // Assert
            Assert.Equal("", row.UniquenessKey);
        }

        [Theory]
        [InlineData("   ", "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "   ", "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "   ", "LINE1", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "   ", "2024-01-15", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "LINE1", "   ", "2024-02-15", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", "   ", "REL1")]
        [InlineData("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", "   ")]
        public void GenerateUniquenessKey_WhitespaceFields_SetsEmptyKey(
            string divisionId, string localSiteId, string poNumber, string poLineNumber,
            string committedDateStr, string requestedDateStr, string releaseOriginalStr)
        {
            // Arrange
            var row = NewRow(divisionId, localSiteId, poNumber, poLineNumber, committedDateStr, requestedDateStr, releaseOriginalStr);

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
            var row1 = NewRow("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1");
            var row2 = NewRow("DIV1", "SITE1", "PO001", "LINE1", "2024-01-15", "2024-02-15", "REL1");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.Equal(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByPONumber_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(poNumber: "PO001");
            var row2 = NewRow(poNumber: "PO002");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByPOLineNumber_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(poLineNumber: "LINE1");
            var row2 = NewRow(poLineNumber: "LINE2");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByCommittedDate_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(committedDateStr: "2024-01-15");
            var row2 = NewRow(committedDateStr: "2024-01-16");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByRequestedDate_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(requestedDateStr: "2024-02-15");
            var row2 = NewRow(requestedDateStr: "2024-02-16");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        [Fact]
        public void GenerateUniquenessKey_TwoRowsDifferingOnlyByRelease_ProduceDifferentKeys()
        {
            // Arrange
            var row1 = NewRow(releaseOriginalStr: "REL1");
            var row2 = NewRow(releaseOriginalStr: "REL2");

            // Act
            row1.GenerateUniquenessKey();
            row2.GenerateUniquenessKey();

            // Assert
            Assert.NotEqual(row1.UniquenessKey, row2.UniquenessKey);
        }

        #endregion
    }
}