using DVT.Core.Models.DataRowEntities;
using DVT.Core.Validators;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ValidatorTests
{
    public class MPNDataRowStaticValidatorTests
    {
        private readonly MPNDataRowStaticValidator _validator = new MPNDataRowStaticValidator();

        private static MPNDataRow ValidRow() => new MPNDataRow
        {
            DivisionID = "0016",
            LocalSiteID = "NI-PEN",
            PartNumber = "100013A-01",
            LocalManufacturerID = "49337",
            ManufactureID = "1724",
            ManufactureName = "QORVO",
            ManufacturerPartNumber = "TGA2526",
            ObjectID = "4CCD851E3F8FF78947BD269D07AB573031D4ADD1",
            MPNType = "P",
            IncorrectColumnCount = false
        };

        // ── Happy path ──────────────────────────────────────────────────────────

        [Fact]
        public void ValidRow_PassesAllRules()
        {
            var result = _validator.Validate(ValidRow());

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("P")]
        [InlineData("S")]
        [InlineData("p")]
        [InlineData("s")]
        public void ValidRow_MPNType_AllowedValues_PassesAllRules(string mpnType)
        {
            var row = ValidRow();
            row.MPNType = mpnType;

            var result = _validator.Validate(row);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidRow_LocalManufacturerID_Empty_PassesAllRules()
        {
            // LocalManufacturerID is optional
            var row = ValidRow();
            row.LocalManufacturerID = string.Empty;

            var result = _validator.Validate(row);

            Assert.True(result.IsValid);
        }

        // ── Mandatory fields ────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(MPNDataRow.DivisionID), MPNFileHeaders.DivisionID)]
        [InlineData(nameof(MPNDataRow.LocalSiteID), MPNFileHeaders.LocalSiteID)]
        [InlineData(nameof(MPNDataRow.PartNumber), MPNFileHeaders.PartNumber)]
        [InlineData(nameof(MPNDataRow.ManufactureID), MPNFileHeaders.ManufactureID)]
        [InlineData(nameof(MPNDataRow.ManufactureName), MPNFileHeaders.ManufactureName)]
        [InlineData(nameof(MPNDataRow.ManufacturerPartNumber), MPNFileHeaders.ManufacturerPartNumber)]
        [InlineData(nameof(MPNDataRow.ObjectID), MPNFileHeaders.ObjectID)]
        [InlineData(nameof(MPNDataRow.MPNType), MPNFileHeaders.MPNType)]
        public void MandatoryField_WhenEmpty_ReturnsError(string propertyName, string expectedPropertyOverride)
        {
            var row = ValidRow();
            typeof(MPNDataRow).GetProperty(propertyName)!.SetValue(row, string.Empty);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── Character limit ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(MPNDataRow.DivisionID), MPNFileHeaders.DivisionID, 257)]
        [InlineData(nameof(MPNDataRow.LocalSiteID), MPNFileHeaders.LocalSiteID, 101)]
        [InlineData(nameof(MPNDataRow.PartNumber), MPNFileHeaders.PartNumber, 257)]
        [InlineData(nameof(MPNDataRow.LocalManufacturerID), MPNFileHeaders.LocalManufacturerID, 21)]
        [InlineData(nameof(MPNDataRow.ManufactureID), MPNFileHeaders.ManufactureID, 21)]
        [InlineData(nameof(MPNDataRow.ManufactureName), MPNFileHeaders.ManufactureName, 129)]
        [InlineData(nameof(MPNDataRow.ManufacturerPartNumber), MPNFileHeaders.ManufacturerPartNumber, 51)]
        [InlineData(nameof(MPNDataRow.ObjectID), MPNFileHeaders.ObjectID, 51)]
        [InlineData(nameof(MPNDataRow.MPNType), MPNFileHeaders.MPNType, 11)]
        public void CharacterLimit_WhenExceeded_ReturnsError(string propertyName, string expectedPropertyOverride, int length)
        {
            var row = ValidRow();
            typeof(MPNDataRow).GetProperty(propertyName)!.SetValue(row, new string('A', length));

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── Invalid format (ASCII-only fields) ──────────────────────────────────

        [Theory]
        [InlineData(nameof(MPNDataRow.PartNumber), MPNFileHeaders.PartNumber)]
        [InlineData(nameof(MPNDataRow.ManufacturerPartNumber), MPNFileHeaders.ManufacturerPartNumber)]
        public void ASCIIField_WithNonAsciiCharacter_ReturnsInvalidFormatError(string propertyName, string expectedPropertyOverride)
        {
            var row = ValidRow();
            typeof(MPNDataRow).GetProperty(propertyName)!.SetValue(row, "VALUE-\u00E9"); // é is non-ASCII

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
            Assert.Equal(ValidationMessages.InvalidFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── MPNType allowed values ───────────────────────────────────────────────

        [Theory]
        [InlineData("X")]
        [InlineData("Z")]
        [InlineData("PS")]
        public void MPNType_UnrecognisedValue_ReturnsInvalidValueError(string mpnType)
        {
            var row = ValidRow();
            row.MPNType = mpnType;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == MPNFileHeaders.MPNType);
            Assert.Equal(ValidationMessages.InvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── IncorrectColumnCount ─────────────────────────────────────────────────

        [Fact]
        public void IncorrectColumnCount_True_ReturnsWarning()
        {
            var row = ValidRow();
            row.IncorrectColumnCount = true;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == IDataRowProperties.IncorrectColumnCount);
            Assert.Equal(ValidationMessages.IncorrectNumberOfColumns, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Warning, failure.ErrorCode);
        }

        // ── CascadeMode (Stop) ───────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(MPNDataRow.DivisionID), MPNFileHeaders.DivisionID)]
        [InlineData(nameof(MPNDataRow.LocalSiteID), MPNFileHeaders.LocalSiteID)]
        [InlineData(nameof(MPNDataRow.PartNumber), MPNFileHeaders.PartNumber)]
        [InlineData(nameof(MPNDataRow.ManufactureID), MPNFileHeaders.ManufactureID)]
        [InlineData(nameof(MPNDataRow.ManufactureName), MPNFileHeaders.ManufactureName)]
        [InlineData(nameof(MPNDataRow.ManufacturerPartNumber), MPNFileHeaders.ManufacturerPartNumber)]
        [InlineData(nameof(MPNDataRow.ObjectID), MPNFileHeaders.ObjectID)]
        [InlineData(nameof(MPNDataRow.MPNType), MPNFileHeaders.MPNType)]
        public void MandatoryField_WhenEmpty_StopsAtFirstFailure(string propertyName, string expectedPropertyOverride)
        {
            // CascadeMode.Stop means only one error is produced per field when the first rule fails
            var row = ValidRow();
            typeof(MPNDataRow).GetProperty(propertyName)!.SetValue(row, string.Empty);

            var result = _validator.Validate(row);

            Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
        }
    }
}