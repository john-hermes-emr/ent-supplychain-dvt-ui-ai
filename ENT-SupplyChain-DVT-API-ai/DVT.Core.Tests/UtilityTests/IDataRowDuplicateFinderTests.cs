using DVT.Core.Helper;
using DVT.Core.Models;
using System.Collections.Generic;
using Xunit;

namespace DVT.Core.Tests.UtilityTests
{
    public class IDataRowDuplicateFinderTests
    {
        #region Helpers

        private static MockDataRow Row(int rowNumber, string key) => new MockDataRow(rowNumber, key);

        private class MockDataRow : IDataRow
        {
            public int RowNumber { get; set; }
            public string UniquenessKey { get; private set; }
            public bool IncorrectColumnCount { get; set; }

            public MockDataRow(int rowNumber, string uniquenessKey)
            {
                RowNumber = rowNumber;
                UniquenessKey = uniquenessKey;
            }

            public void GenerateUniquenessKey() { }            
        }

        #endregion

        [Fact]
        public void FindDuplicatesRowNumbers_EmptyInput_ReturnsEmptyList()
        {
            // Arrange
            var dataRows = new List<IDataRow>();

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindDuplicatesRowNumbers_AllUniqueRows_ReturnsEmptyList()
        {
            // Arrange
            var dataRows = new List<IDataRow>
            {
                Row(0, "key-A"),
                Row(1, "key-B"),
                Row(2, "key-C")
            };

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindDuplicatesRowNumbers_OneDuplicatePair_ReturnsBothRowNumbers()
        {
            // Arrange
            var dataRows = new List<IDataRow>
            {
                Row(0, "key-A"),
                Row(1, "key-B"),
                Row(2, "key-A")  // duplicate of row 0
            };

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(0, result); // first occurrence added when duplicate found
            Assert.Contains(2, result); // the duplicate row
        }

        [Fact]
        public void FindDuplicatesRowNumbers_MultipleDistinctDuplicates_ReturnsAllDuplicateRowNumbers()
        {
            // Arrange
            var dataRows = new List<IDataRow>
            {
                Row(0, "key-A"),
                Row(1, "key-B"),
                Row(2, "key-A"),  // duplicate of row 0
                Row(3, "key-B"),  // duplicate of row 1
            };

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Contains(0, result);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
            Assert.Contains(3, result);
        }
        
        [Fact]
        public void FindDuplicatesRowNumbers_SingleRow_ReturnsEmptyList()
        {
            // Arrange
            var dataRows = new List<IDataRow>
            {
                Row(0, "key-A")
            };

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindDuplicatesRowNumbers_AllRowsDuplicate_ReturnsAllRowNumbers()
        {
            // Arrange
            var dataRows = new List<IDataRow>
            {
                Row(0, "key-A"),
                Row(1, "key-A"),
                Row(2, "key-A"),
                Row(3, "key-A")
            };

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Contains(0, result);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
            Assert.Contains(3, result);
        }

        [Fact]
        public void FindDuplicatesRowNumbers_DuplicatesNotAdjacent_StillDetected()
        {
            // Arrange
            var dataRows = new List<IDataRow>
            {
                Row(0, "key-A"),
                Row(1, "key-B"),
                Row(2, "key-C"),
                Row(3, "key-B"),  // duplicate of row 1 (non-adjacent)
            };

            // Act
            var result = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(3, result);
        }
    }
}