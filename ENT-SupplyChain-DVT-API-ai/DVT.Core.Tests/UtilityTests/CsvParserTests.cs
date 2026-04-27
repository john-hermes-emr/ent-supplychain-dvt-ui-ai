using DocumentFormat.OpenXml.Spreadsheet;
using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DVT.Core.Tests.UtilityTests
{
    public class CsvParserTests
    {
        [Fact]
        public void TryReadRow_EmptyData_ReturnsFalse()
        {
            // Arrange
            var parser = new EfficientCsvParser(ReadOnlySpan<char>.Empty);

            // Act
            var result = parser.TryReadRow(out var row);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryReadRow_SingleRow_ReturnsTrue()
        {
            // Arrange
            var data = "field1|field2|field3|\r\n".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act
            var result = parser.TryReadRow(out var row);

            // Assert
            Assert.True(result);
            Assert.Equal(3, row.FieldCount);
        }

        [Fact]
        public void TryReadRow_MultipleRows_ReadsAllRowsSequentially()
        {
            // Arrange
            var data = "row1field1|row1field2|\nrow2field1|row2field2|\nrow3field1|row3field2|".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act & Assert
            Assert.True(parser.TryReadRow(out var row1));
            Assert.Equal(2, row1.FieldCount);
            Assert.Equal("row1field1", row1.GetField(0).ToString());
            Assert.Equal("row1field2", row1.GetField(1).ToString());

            Assert.True(parser.TryReadRow(out var row2));
            Assert.Equal(2, row2.FieldCount);
            Assert.Equal("row2field1", row2.GetField(0).ToString());
            Assert.Equal("row2field2", row2.GetField(1).ToString());

            Assert.True(parser.TryReadRow(out var row3));
            Assert.Equal(2, row3.FieldCount);
            Assert.Equal("row3field1", row3.GetField(0).ToString());
            Assert.Equal("row3field2", row3.GetField(1).ToString());

            Assert.False(parser.TryReadRow(out _));
        }

        [Fact]
        public void TryReadRow_WindowsLineEndings_HandlesCarriageReturn()
        {
            // Arrange
            var data = "field1|field2|\r\nfield3|field4|\r\n".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act
            parser.TryReadRow(out var row1);
            parser.TryReadRow(out var row2);

            // Assert
            Assert.Equal("field1", row1.GetField(0).ToString());
            Assert.Equal("field2", row1.GetField(1).ToString());
            Assert.Equal("field3", row2.GetField(0).ToString());
            Assert.Equal("field4", row2.GetField(1).ToString());
        }

        [Fact]
        public void TryReadRow_MixedLineEndings_HandlesCorrectly()
        {
            // Arrange
            var data = "field1|field2|\nfield3|field4|\r\nfield5|field6|\r\n".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act & Assert
            Assert.True(parser.TryReadRow(out var row1));
            Assert.Equal("field1", row1.GetField(0).ToString());

            Assert.True(parser.TryReadRow(out var row2));
            Assert.Equal("field3", row2.GetField(0).ToString());

            Assert.True(parser.TryReadRow(out var row3));
            Assert.Equal("field5", row3.GetField(0).ToString());
        }

        [Fact]
        public void TryReadRow_NoTrailingNewline_ReadsLastRow()
        {
            // Arrange
            var data = "field1|field2|\nfield3|field4|".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act
            parser.TryReadRow(out var row1);
            var hasSecondRow = parser.TryReadRow(out var row2);

            // Assert
            Assert.True(hasSecondRow);
            Assert.Equal("field3", row2.GetField(0).ToString());
            Assert.Equal("field4", row2.GetField(1).ToString());
            Assert.False(parser.TryReadRow(out _));
        }

        [Fact]
        public void TryReadRow_EmptyLines_HandlesCorrectly()
        {
            // Arrange
            var data = "field1|field2|\n\nfield3|field4|".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act
            parser.TryReadRow(out var row1);
            parser.TryReadRow(out var emptyRow);
            parser.TryReadRow(out var row3);

            // Assert
            Assert.True(emptyRow.IsEmptyLine);
            Assert.Equal(1, emptyRow.FieldCount);            
            Assert.Equal("field3", row3.GetField(0).ToString());

            //Getting fields from empty row throws an exception
            try
            {
                emptyRow.GetField(0);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("index", ex.ParamName);
                Assert.Contains("Specified argument was out of the range of valid values. (Parameter 'index')", ex.Message);
                return;
            }
        }

        [Fact]
        public void CsvRow_FieldCount_CountsCorrectly()
        {
            // Arrange
            var line = "field1|field2|field3|field4|field5|".AsSpan();
            var row = new CsvRow(line);

            // Act
            var count = row.FieldCount;

            // Assert
            Assert.Equal(5, count);
        }

        [Fact]
        public void CsvRow_FieldCount_SingleField_ReturnsOne()
        {
            // Arrange
            var line = "onlyfield".AsSpan();
            var row = new CsvRow(line);

            // Act
            var count = row.FieldCount;

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void CsvRow_FieldCount_EmptyLine_ReturnsOne()
        {
            // Arrange
            var line = "".AsSpan();
            var row = new CsvRow(line);

            // Act
            var count = row.FieldCount;

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void CsvRow_FieldCount_LazyEvaluation_CachesValue()
        {
            // Arrange
            var line = "field1|field2|field3|".AsSpan();
            var row = new CsvRow(line);

            // Act
            var count1 = row.FieldCount;
            var count2 = row.FieldCount;

            // Assert
            Assert.Equal(3, count1);
            Assert.Equal(3, count2);
        }

        [Fact]
        public void CsvRow_GetField_RetrievesCorrectField()
        {
            // Arrange
            var line = "alpha|beta|gamma|delta|".AsSpan();
            var row = new CsvRow(line);

            // Act & Assert
            Assert.Equal("alpha", row.GetField(0).ToString());
            Assert.Equal("beta", row.GetField(1).ToString());
            Assert.Equal("gamma", row.GetField(2).ToString());
            Assert.Equal("delta", row.GetField(3).ToString());
        }

        [Fact]
        public void CsvRow_GetField_EmptyFields_ReturnsEmpty()
        {
            // Arrange
            var line = "field1||field3|".AsSpan();
            var row = new CsvRow(line);

            // Act & Assert
            Assert.Equal("field1", row.GetField(0).ToString());
            Assert.Equal("", row.GetField(1).ToString());
            Assert.Equal("field3", row.GetField(2).ToString());            
        }

        [Fact]
        public void CsvRow_GetField_FirstField_ReturnsCorrectValue()
        {
            // Arrange
            var line = "first|second|third|".AsSpan();
            var row = new CsvRow(line);

            // Act
            var field = row.GetField(0);

            // Assert
            Assert.Equal("first", field.ToString());
        }

        [Fact]
        public void CsvRow_GetField_LastField_ReturnsCorrectValue()
        {
            // Arrange
            var line = "first|second|third|".AsSpan();
            var row = new CsvRow(line);

            // Act
            var field = row.GetField(2);

            // Assert
            Assert.Equal("third", field.ToString());
        }

        [Fact]
        public void CsvRow_GetField_IndexOutOfRange_ThrowsException()
        {
            // Arrange
            var line = "field1|field2|".AsSpan();
            var row = new CsvRow(line);

            try
            {
                row.GetField(5);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("index", ex.ParamName);
                Assert.Contains("Specified argument was out of the range of valid values. (Parameter 'index')", ex.Message);
                return;
            }
        }

        [Fact]
        public void CsvRow_GetFieldOrDefault_IndexOutOfRange_ReturnsEmpty()
        {
            // Arrange
            var line = "field1|field2|".AsSpan();
            var row = new CsvRow(line);
            var value = row.GetFieldOrDefault(5);

            Assert.Equal("", value.ToString());
        }

        [Fact]
        public void CsvRow_GetField_NegativeIndex_ThrowsException()
        {
            // Arrange
            var line = "field1|field2".AsSpan();
            var row = new CsvRow(line);

            try
            {
                row.GetField(-1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("index", ex.ParamName);
                Assert.Contains("Specified argument was out of the range of valid values. (Parameter 'index')", ex.Message);
                return;
            }                
        }

        [Fact]
        public void CsvRow_GetAsSplitStringList_ReturnsTrimmedFields()
        {
            // Arrange
            var line = "  field1  | field2 |field3  ".AsSpan();
            var row = new CsvRow(line);

            // Act
            var result = row.GetAsSplitStringList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("field1", result[0]);
            Assert.Equal("field2", result[1]);
            Assert.Equal("field3", result[2]);
        }

        [Fact]
        public void CsvRow_GetAsSplitStringList_EmptyFields_ReturnsEmptyStrings()
        {
            // Arrange
            var line = "field1||field3".AsSpan();
            var row = new CsvRow(line);

            // Act
            var result = row.GetAsSplitStringList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("field1", result[0]);
            Assert.Equal("", result[1]);
            Assert.Equal("field3", result[2]);
        }

        [Fact]
        public void CsvRow_GetAsSplitStringList_SingleField_ReturnsSingleItem()
        {
            // Arrange
            var line = "onlyfield".AsSpan();
            var row = new CsvRow(line);

            // Act
            var result = row.GetAsSplitStringList();

            // Assert
            Assert.Single(result);
            Assert.Equal("onlyfield", result[0]);
        }

        [Fact]
        public void TryReadRow_LargeDataSet_ParsesAllRows()
        {
            // Arrange
            var data = CsvParserTestData.SupplierCsvContent;
            var parser = new EfficientCsvParser(data.AsSpan());
            var rows = new List<SupplierDataRow>();

            // Act
            while (parser.TryReadRow(out var row))
            {
                rows.Add(new SupplierDataRow()
                {
                    SupplierId = row.GetField(2).ToString(),
                    SupplierName = row.GetField(3).ToString()
                });
            }

            // Assert
            Assert.Equal(10, rows.Count); // 1 header + 9 data rows

            // Verify header
            Assert.True(rows[0].SupplierId == "Supplier ID");
            Assert.True(rows[0].SupplierName == "Supplier Name");


            // Verify first data row
            Assert.Equal("104673439429", rows[1].SupplierId);
            Assert.Equal("MCMASTER CARR SUPPLY CO", rows[1].SupplierName);
        }

        [Fact]
        public void CsvRow_GetField_SpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var line = "field with spaces|field-with-dashes|field_with_underscores".AsSpan();
            var row = new CsvRow(line);

            // Act & Assert
            Assert.Equal("field with spaces", row.GetField(0).ToString());
            Assert.Equal("field-with-dashes", row.GetField(1).ToString());
            Assert.Equal("field_with_underscores", row.GetField(2).ToString());
        }

        [Fact]
        public void TryReadRow_OnlyNewlines_ThrowsExceptions()
        {
            // Arrange
            var data = "\n\n\n".AsSpan();
            var parser = new EfficientCsvParser(data);

            // Act & Assert
            Assert.True(parser.TryReadRow(out var row1));

            try
            {
                Assert.Equal("", row1.GetField(0).ToString());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("index", ex.ParamName);
                Assert.Contains("Specified argument was out of the range of valid values. (Parameter 'index')", ex.Message);
                return;
            }            

            Assert.True(parser.TryReadRow(out var row2));

            try
            {
                Assert.Equal("", row2.GetField(0).ToString());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("index", ex.ParamName);
                Assert.Contains("Specified argument was out of the range of valid values. (Parameter 'index')", ex.Message);
                return;
            }

            Assert.True(parser.TryReadRow(out var row3));

            try
            {
                Assert.Equal("", row3.GetField(0).ToString());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.Equal("index", ex.ParamName);
                Assert.Contains("Specified argument was out of the range of valid values. (Parameter 'index')", ex.Message);
                return;
            }

            Assert.False(parser.TryReadRow(out _));
        }
    }

    public class CsvParserTestData
    {
        public static string SupplierCsvContent { get; private set; } = @"Division ID|Local Site ID|Supplier ID|Supplier Name|DUNS|Active_inactive|Direct_indirect|Address Descr|Street|Suite|City|State|Postal Code|County|Country|Addr1|Addr2|Addr3|Addr4|Country Code|Global Flag|Main Telephone|Toll Free|Fax|Web site|Supplier Type|
0047|FVLV-077|104673439429|MCMASTER CARR SUPPLY CO|0|A|D||||CHICAGO|IL|60680-7690||UNITED STATES|PO BOX 7690||CHICAGO,IL,60680-7690||US|U|999 888 777||||M|
0047|FVLV-077|11842854941184|D PRECIZE TECHNOLOGY|0|A|D||||SINGAPORE||408704||SINGAPORE|BLK 3015 UBI RD 1 02 214||SINGAPORE,NULL,408704||SG|R|999 888 777||||M|
0047|FVLV-077|129869439760|BARTEC PTE LTD|0|A|D||||SINGAPORE||669569||SINGAPORE|63 HILLVIEW AVE|07 20 LAM SOON INDUSTRIAL BLDG|SINGAPORE,NULL,669569||SG|R|999 888 777||||M|
0047|FVLV-077|1855294369119|FUTURE ELECTRONICS INC DISTRIBUTION PTE LTD|0|A|D||||SINGAPORE||486015||SINGAPORE|2 CHANGI BUSINESS PARK AVENUE 1|06 51 PARK AVENUE CHANGI|SINGAPORE,NULL,486015||SG|G|999 888 777||||M|
0047|FVLV-077|41753654434177|SHENZHEN COLIBRI PRECISION MANUFACTURING CO LTD|0|A|D||||SHENZHEN|GUANGDONG|518100||CHINA|BLDG A1 A BAO INTERNATIONAL OPTICAL COMMUNICATION INDUSTRIAL YARD|4 INDUSTRIAL PARK SHUITIAN VLG|SHENZHEN,NULL,518100||CN|G|999 888 777||||M|
0047|FVLV-077|41994044434368|ICONNEXION ASIA PTE LTD|0|A|D||||SINGAPORE||569139||SINGAPORE|3 ANG MO KIO ST 62|01 57 LINK AT AMK|SINGAPORE,NULL,569139||SG|R|999 888 777||||M|
0047|FVLV-077|50502655425122|XFMRS LTD|0|A|D||||KWAI CHUNG|NEW TERRITORIES|0||HONG KONG|BLK G H I J 3F PHASE I GOLDEN DRAGON IND CTR|152 160 TAI LIN PAI RD|KWAI CHUNG,NULL,NULL||HK|R|(852)2423 1689||(852)2420 3236||M|
0047|FVLV-077|59788439946|DIA-COM CORP|0|A|D||||AMHERST|NH|03031-2315||UNITED STATES|5 HOWE DR||AMHERST,NH,03031-2315||US|G|999 888 777||||M|
0047|FVLV-077|10492651189065|COLIBRI AUTOMATION THAILAND CO LTD|0|A|D||||KHLONG LUANG|PATHUM THANI|12120||THAILAND|19/56-58, MOO 10, PHAHOLYOTHIN RD,|KHLONG NUENG|KHLONG LUANG,PATHUM THANI,12120||TH|G|(66)29081671||(65)25293125||M|";

        public static IEnumerable<object[]> SupplierCsvRows =>
            new List<object[]>
            {
                new object[] { "Division ID|Local Site ID|Supplier ID", 1 },
                new object[] { "field1|field2\nfield3|field4", 2 },
                new object[] { "a|b|c\nd|e|f\ng|h|i", 3 },
                new object[] { "", 0 },
                new object[] { "single", 1 }
            };
    }
}
