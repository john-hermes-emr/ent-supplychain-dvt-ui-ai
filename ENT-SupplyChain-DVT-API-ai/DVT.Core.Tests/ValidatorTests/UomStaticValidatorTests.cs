using DVT.Core.Models.DataRowEntities;
using DVT.Core.Validators;
using System.Linq;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ValidatorTests
{
    public class UomStaticValidatorTests
    {
        private readonly UOMDataRowStaticValidator _validator = new();

        /// <summary>
        /// Builds a fully valid <see cref="UOMDataRow"/> that passes all rules.
        /// Individual tests mutate specific fields to exercise failure paths.
        /// </summary>
        private static UOMDataRow ValidRow() => new()
        {
            DivisionID = "DIV001",
            LocalSiteID = "SITE001",
            PartNumber = "PART123",
            LocalUOM = "EA",
            BaseUOM = "EA",
            ConversionRate = 1.0m,
            IncorrectColumnCount = false
        };

        // ── Valid row ────────────────────────────────────────────────────────────

        [Fact]
        public void ValidRow_PassesAllRules()
        {
            var result = _validator.Validate(ValidRow());
            Assert.True(result.IsValid);
        }

        // ── Mandatory fields ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(UOMDataRow.DivisionID), UOMFileHeaders.DivisionID)]
        [InlineData(nameof(UOMDataRow.LocalSiteID), UOMFileHeaders.LocalSiteID)]
        [InlineData(nameof(UOMDataRow.PartNumber), UOMFileHeaders.PartNumber)]
        [InlineData(nameof(UOMDataRow.LocalUOM), UOMFileHeaders.LocalUOM)]
        [InlineData(nameof(UOMDataRow.BaseUOM), UOMFileHeaders.BaseUOM)]
        public void MandatoryField_WhenEmpty_ReturnsError(string propertyName, string expectedPropertyOverride)
        {
            var row = ValidRow();
            typeof(UOMDataRow).GetProperty(propertyName)!.SetValue(row, string.Empty);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── Character limit ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(UOMDataRow.DivisionID), UOMFileHeaders.DivisionID, 101)]
        [InlineData(nameof(UOMDataRow.LocalSiteID), UOMFileHeaders.LocalSiteID, 101)]
        [InlineData(nameof(UOMDataRow.PartNumber), UOMFileHeaders.PartNumber, 51)]
        [InlineData(nameof(UOMDataRow.LocalUOM), UOMFileHeaders.LocalUOM, 21)]
        [InlineData(nameof(UOMDataRow.BaseUOM), UOMFileHeaders.BaseUOM, 21)]
        public void CharacterLimit_WhenExceeded_ReturnsError(string propertyName, string expectedPropertyOverride, int overLength)
        {
            var row = ValidRow();
            typeof(UOMDataRow).GetProperty(propertyName)!.SetValue(row, new string('A', overLength));

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f =>
                f.PropertyName == expectedPropertyOverride &&
                f.ErrorMessage == ValidationMessages.CharacterLimitHasBeenExceeded);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── CascadeMode.Stop – only first failure per field reported ─────────────

        [Fact]
        public void CascadeStop_WhenPartNumberIsEmpty_OnlyMandatoryErrorReported()
        {
            // PartNumber is mandatory AND has ASCII format check.
            // With CascadeMode.Stop only the MandatoryField error should surface.
            var row = ValidRow();
            row.PartNumber = string.Empty;

            var result = _validator.Validate(row);

            var failures = result.Errors.Where(f => f.PropertyName == UOMFileHeaders.PartNumber).ToList();
            Assert.Single(failures);
            Assert.Equal(ValidationMessages.MandatoryField, failures[0].ErrorMessage);
        }

        // ── PartNumber – ASCII format ────────────────────────────────────────────

        [Fact]
        public void PartNumber_WithNonAsciiCharacters_ReturnsInvalidFormat()
        {
            var row = ValidRow();
            row.PartNumber = "PART\u4E2D\u6587"; // contains non-ASCII characters

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == UOMFileHeaders.PartNumber);
            Assert.Equal(ValidationMessages.InvalidFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void PartNumber_WithAsciiOnlyCharacters_PassesFormatRule()
        {
            var row = ValidRow();
            row.PartNumber = "PART-001_ABC";

            Assert.True(_validator.Validate(row).IsValid);
        }

        // ── ConversionRate ───────────────────────────────────────────────────────

        [Theory]
        [InlineData(ErrorTypes.MandatoryField, "", "MANDATORY FIELD, VALUE REQUIRED")]
        [InlineData(ErrorTypes.CharacterLimitExceeded, "999999999999", "CHARACTER LIMIT HAS BEEN EXCEEDED")]
        [InlineData(ErrorTypes.InvalidValue, "not-a-number", "INVALID VALUE")]
        [InlineData(ErrorTypes.InvalidFormat, "abc", "INVALID VALUE")]
        public void ConversionRate_WhenNullWithErrorType_ReturnsExpectedMessage(
            ErrorTypes errorType, string originalStr, string expectedMessage)
        {
            var row = ValidRow();
            row.ConversionRate = null;
            row.ConversionRateError = errorType;
            row.ConversionRateOriginalStr = originalStr;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == UOMFileHeaders.ConversionRate);
            Assert.Equal(expectedMessage, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void ConversionRate_WhenNullWithMandatoryFieldError_AttemptedValueIsEmpty()
        {
            var row = ValidRow();
            row.ConversionRate = null;
            row.ConversionRateError = ErrorTypes.MandatoryField;

            var result = _validator.Validate(row);

            var failure = Assert.Single(result.Errors, f => f.PropertyName == UOMFileHeaders.ConversionRate);
            Assert.Equal(string.Empty, failure.AttemptedValue);
        }

        [Fact]
        public void ConversionRate_WhenNullWithParseError_AttemptedValueIsOriginalStr()
        {
            var row = ValidRow();
            row.ConversionRate = null;
            row.ConversionRateError = ErrorTypes.InvalidValue;
            row.ConversionRateOriginalStr = "not-a-number";

            var result = _validator.Validate(row);

            var failure = Assert.Single(result.Errors, f => f.PropertyName == UOMFileHeaders.ConversionRate);
            Assert.Equal(row.ConversionRateOriginalStr, failure.AttemptedValue);
        }

        [Fact]
        public void ConversionRate_WhenHasValue_NoFailureReported()
        {
            var row = ValidRow();
            row.ConversionRate = 2.5m;

            var result = _validator.Validate(row);

            Assert.DoesNotContain(result.Errors, f => f.PropertyName == UOMFileHeaders.ConversionRate);
        }

        // ── IncorrectColumnCount ─────────────────────────────────────────────────

        [Fact]
        public void IncorrectColumnCount_WhenTrue_ReturnsWarning()
        {
            var row = ValidRow();
            row.IncorrectColumnCount = true;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == IDataRowProperties.IncorrectColumnCount);
            Assert.Equal(ValidationMessages.IncorrectNumberOfColumns, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Warning, failure.ErrorCode);
        }

        [Fact]
        public void IncorrectColumnCount_WhenFalse_PassesRule()
        {
            var row = ValidRow();
            row.IncorrectColumnCount = false;

            Assert.True(_validator.Validate(row).IsValid);
        }

        // ── Multiple failures ────────────────────────────────────────────────────

        [Fact]
        public void MultipleEmptyFields_ReturnsFailureForEachField()
        {
            var row = new UOMDataRow
            {
                DivisionID = string.Empty,
                LocalSiteID = string.Empty,
                PartNumber = string.Empty,
                LocalUOM = string.Empty,
                BaseUOM = string.Empty,
                ConversionRate = null,
                ConversionRateError = ErrorTypes.MandatoryField,
                IncorrectColumnCount = false
            };

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, f => f.PropertyName == UOMFileHeaders.DivisionID);
            Assert.Contains(result.Errors, f => f.PropertyName == UOMFileHeaders.LocalSiteID);
            Assert.Contains(result.Errors, f => f.PropertyName == UOMFileHeaders.PartNumber);
            Assert.Contains(result.Errors, f => f.PropertyName == UOMFileHeaders.LocalUOM);
            Assert.Contains(result.Errors, f => f.PropertyName == UOMFileHeaders.BaseUOM);
            Assert.Contains(result.Errors, f => f.PropertyName == UOMFileHeaders.ConversionRate);
        }
    }
}