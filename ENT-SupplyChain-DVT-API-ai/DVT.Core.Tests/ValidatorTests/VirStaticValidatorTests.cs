using DVT.Core.Helper;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Validators;
using System;
using Xunit;
using static DVT.Core.Constants;

namespace DVT.Core.Tests.ValidatorTests
{
    public class VirDataRowStaticValidatorTests
    {
        private readonly VirDataRowStaticValidator _validator = new VirDataRowStaticValidator();

        private static VirDataRow ValidRow() => new VirDataRow
        {
            DivisionId = "DIV001",
            LocalSiteId = "NI-PEN",
            PoNumber = "PO-12345",
            POLineNumber = "1",
            SupplierId = "SUP001",
            PartNumber = "PART-001",
            SupplierPartNumber = "SPART-001",
            ReceiptNumber = "REC001",
            QuantityOrdered = (BigDecimal)10,
            QuantityOrderedOriginalStr = "10",
            QuantityOrderedError = ErrorTypes.None,
            QuantityReceived = (BigDecimal)5,
            QuantityReceivedOriginalStr = "5",
            QuantityReceivedError = ErrorTypes.None,
            DateReceived = new DateTime(DateTime.UtcNow.Year - 1, 1, 1),
            DateReceivedStr = "01/01/2023",
            DateReceivedError = "",
            InvoicePricePaid = (BigDecimal)100,
            InvoicePricePaidOriginalStr = "100",
            InvoicePricePaidError = ErrorTypes.None,
            UnitPrice = (BigDecimal)20,
            UnitPriceOriginalStr = "20",
            UnitPriceError = ErrorTypes.None,
            PureLoadedCost = "10.00",
            CurrencyCode = "USD",
            IntraDiv = "Y",
            DirectIndirect = "D",
            POTerms = "NET30",
            FreightTerms = "FOB",
            UOM = "EA",
            TitleTransfer = "DEST",
            Port = "NY",
            Release = (BigDecimal)1,
            ReleaseOriginalStr = "1",
            ReleaseError = ErrorTypes.None,
            CommittedDate = new DateTime(DateTime.UtcNow.Year - 1, 6, 1),
            CommittedDateStr = "06/01/2023",
            CommittedDateError = "",
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

        [Fact]
        public void ValidRow_OptionalFields_Empty_PassesAllRules()
        {
            var row = ValidRow();
            row.SupplierPartNumber = string.Empty;
            row.FreightTerms = string.Empty;
            row.TitleTransfer = string.Empty;
            row.Port = string.Empty;
            row.QuantityOrdered = null;
            row.QuantityOrderedError = ErrorTypes.MandatoryField;
            row.Release = null;
            row.ReleaseError = ErrorTypes.None;

            var result = _validator.Validate(row);

            Assert.True(result.IsValid);
        }

        // ── Mandatory fields ────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(VirDataRow.DivisionId), VirFileHeaders.DivisionId)]
        [InlineData(nameof(VirDataRow.LocalSiteId), VirFileHeaders.LocalSiteId)]
        [InlineData(nameof(VirDataRow.PoNumber), VirFileHeaders.PoNumber)]
        [InlineData(nameof(VirDataRow.POLineNumber), VirFileHeaders.PoLineNumber)]
        [InlineData(nameof(VirDataRow.SupplierId), VirFileHeaders.SupplierId)]
        [InlineData(nameof(VirDataRow.PartNumber), VirFileHeaders.PartNumber)]
        [InlineData(nameof(VirDataRow.PureLoadedCost), VirFileHeaders.PureLoadedCost)]
        [InlineData(nameof(VirDataRow.CurrencyCode), VirFileHeaders.CurrencyCode)]
        [InlineData(nameof(VirDataRow.IntraDiv), VirFileHeaders.IntraDiv)]
        [InlineData(nameof(VirDataRow.DirectIndirect), VirFileHeaders.DirectIndirect)]
        [InlineData(nameof(VirDataRow.POTerms), VirFileHeaders.PoTerms)]
        [InlineData(nameof(VirDataRow.UOM), VirFileHeaders.Uom)]
        public void MandatoryField_WhenEmpty_ReturnsError(string propertyName, string expectedPropertyOverride)
        {
            var row = ValidRow();
            typeof(VirDataRow).GetProperty(propertyName)!.SetValue(row, string.Empty);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void ReceiptNumber_WhenEmpty_ReturnsMandatoryFieldError()
        {
            var row = ValidRow();
            row.ReceiptNumber = string.Empty;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.ReceiptNumber);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void QuantityReceived_WhenNullAndMandatoryFieldError_ReturnsMandatoryFieldError()
        {
            var row = ValidRow();
            row.QuantityReceived = null;
            row.QuantityReceivedError = ErrorTypes.MandatoryField;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.QuantityReceived);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void InvoicePricePaid_WhenNullAndMandatoryFieldError_ReturnsMandatoryFieldError()
        {
            var row = ValidRow();
            row.InvoicePricePaid = null;
            row.InvoicePricePaidError = ErrorTypes.MandatoryField;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.InvoicePricePaid);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void UnitPrice_WhenNullAndMandatoryFieldError_ReturnsMandatoryFieldError()
        {
            var row = ValidRow();
            row.UnitPrice = null;
            row.UnitPriceError = ErrorTypes.MandatoryField;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.UnitPrice);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void DateReceived_WhenNullAndMandatoryFieldError_ReturnsMandatoryFieldError()
        {
            var row = ValidRow();
            row.DateReceived = null;
            row.DateReceivedError = ValidationMessages.MandatoryField;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.DateReceived);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void CommittedDate_WhenNullAndMandatoryFieldError_ReturnsMandatoryFieldError()
        {
            var row = ValidRow();
            row.CommittedDate = null;
            row.CommittedDateError = ValidationMessages.MandatoryField;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.CommittedDate);
            Assert.Equal(ValidationMessages.MandatoryField, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── Character limit ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(nameof(VirDataRow.DivisionId), VirFileHeaders.DivisionId, 101)]
        [InlineData(nameof(VirDataRow.LocalSiteId), VirFileHeaders.LocalSiteId, 101)]
        [InlineData(nameof(VirDataRow.PoNumber), VirFileHeaders.PoNumber, 51)]
        [InlineData(nameof(VirDataRow.POLineNumber), VirFileHeaders.PoLineNumber, 51)]
        [InlineData(nameof(VirDataRow.SupplierId), VirFileHeaders.SupplierId, 101)]
        [InlineData(nameof(VirDataRow.PartNumber), VirFileHeaders.PartNumber, 51)]
        [InlineData(nameof(VirDataRow.SupplierPartNumber), VirFileHeaders.SupplierPartNumber, 51)]
        [InlineData(nameof(VirDataRow.PureLoadedCost), VirFileHeaders.PureLoadedCost, 51)]
        [InlineData(nameof(VirDataRow.CurrencyCode), VirFileHeaders.CurrencyCode, 11)]
        [InlineData(nameof(VirDataRow.IntraDiv), VirFileHeaders.IntraDiv, 11)]
        [InlineData(nameof(VirDataRow.DirectIndirect), VirFileHeaders.DirectIndirect, 11)]
        [InlineData(nameof(VirDataRow.POTerms), VirFileHeaders.PoTerms, 129)]
        [InlineData(nameof(VirDataRow.FreightTerms), VirFileHeaders.FreightTerms, 51)]
        [InlineData(nameof(VirDataRow.UOM), VirFileHeaders.Uom, 21)]
        [InlineData(nameof(VirDataRow.TitleTransfer), VirFileHeaders.TitleTransfer, 51)]
        [InlineData(nameof(VirDataRow.Port), VirFileHeaders.Port, 11)]
        public void CharacterLimit_WhenExceeded_ReturnsError(string propertyName, string expectedPropertyOverride, int length)
        {
            var row = ValidRow();
            typeof(VirDataRow).GetProperty(propertyName)!.SetValue(row, new string('A', length));

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == expectedPropertyOverride);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void ReceiptNumber_WhenExceedsCharacterLimit_ReturnsError()
        {
            var row = ValidRow();
            row.ReceiptNumber = new string('A', 51);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.ReceiptNumber);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void QuantityReceived_WhenNullAndCharacterLimitExceeded_ReturnsError()
        {
            var row = ValidRow();
            row.QuantityReceived = null;
            row.QuantityReceivedError = ErrorTypes.CharacterLimitExceeded;
            row.QuantityReceivedOriginalStr = new string('9', 51);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.QuantityReceived);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void QuantityOrdered_WhenNullAndCharacterLimitExceeded_ReturnsError()
        {
            var row = ValidRow();
            row.QuantityOrdered = null;
            row.QuantityOrderedError = ErrorTypes.CharacterLimitExceeded;
            row.QuantityOrderedOriginalStr = new string('9', 51);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.QuantityOrdered);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void InvoicePricePaid_WhenNullAndCharacterLimitExceeded_ReturnsError()
        {
            var row = ValidRow();
            row.InvoicePricePaid = null;
            row.InvoicePricePaidError = ErrorTypes.CharacterLimitExceeded;
            row.InvoicePricePaidOriginalStr = new string('9', 51);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.InvoicePricePaid);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void UnitPrice_WhenNullAndCharacterLimitExceeded_ReturnsError()
        {
            var row = ValidRow();
            row.UnitPrice = null;
            row.UnitPriceError = ErrorTypes.CharacterLimitExceeded;
            row.UnitPriceOriginalStr = new string('9', 51);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.UnitPrice);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── ReceiptNumber ASCII validation ──────────────────────────────────────

        [Fact]
        public void ReceiptNumber_WithNonAsciiCharacter_ReturnsInvalidFormatError()
        {
            var row = ValidRow();
            row.ReceiptNumber = "REC-\u00E9"; // é is non-ASCII

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.ReceiptNumber);
            Assert.Equal(ValidationMessages.InvalidFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── DateReceived validation ─────────────────────────────────────────────

        [Fact]
        public void DateReceived_WhenCurrentMonth_ReturnsIncorrectDateFormatError()
        {
            var row = ValidRow();
            row.DateReceived = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            row.DateReceivedStr = row.DateReceived.Value.ToString("MM/dd/yyyy");

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.DateReceived);
            Assert.Equal(ValidationMessages.IncorrectDateFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void DateReceived_WhenFutureMonth_ReturnsIncorrectDateFormatError()
        {
            var row = ValidRow();
            row.DateReceived = DateTime.UtcNow.AddMonths(1);
            row.DateReceivedStr = row.DateReceived.Value.ToString("MM/dd/yyyy");

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.DateReceived);
            Assert.Equal(ValidationMessages.IncorrectDateFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void DateReceived_WhenNullAndNonMandatoryError_ReturnsDateError()
        {
            var row = ValidRow();
            row.DateReceived = null;
            row.DateReceivedStr = "99/99/9999";
            row.DateReceivedError = ValidationMessages.IncorrectDateFormat;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.DateReceived);
            Assert.Equal(ValidationMessages.IncorrectDateFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── CommittedDate validation ────────────────────────────────────────────

        [Fact]
        public void CommittedDate_WhenNullAndNonMandatoryError_ReturnsDateError()
        {
            var row = ValidRow();
            row.CommittedDate = null;
            row.CommittedDateStr = "99/99/9999";
            row.CommittedDateError = ValidationMessages.IncorrectDateFormat;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.CommittedDate);
            Assert.Equal(ValidationMessages.IncorrectDateFormat, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── QuantityReceived value-is-zero ──────────────────────────────────────

        [Fact]
        public void QuantityReceived_WhenValueIsZeroError_ReturnsValueIsZeroError()
        {
            var row = ValidRow();
            row.QuantityReceivedError = ErrorTypes.ValueIsZero;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.QuantityReceived);
            Assert.Equal(VirFileHeaders.QuantityReceived + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void QuantityReceived_WhenNullAndInvalidFormatError_ReturnsValueIsZeroError()
        {
            var row = ValidRow();
            row.QuantityReceived = null;
            row.QuantityReceivedError = ErrorTypes.InvalidFormat;
            row.QuantityReceivedOriginalStr = "0";

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.QuantityReceived);
            Assert.Equal(VirFileHeaders.QuantityReceived + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── QuantityOrdered invalid value ───────────────────────────────────────

        [Fact]
        public void QuantityOrdered_WhenNullAndNotMandatoryOrCharacterLimit_ReturnsInvalidValueError()
        {
            var row = ValidRow();
            row.QuantityOrdered = null;
            row.QuantityOrderedError = ErrorTypes.InvalidFormat;
            row.QuantityOrderedOriginalStr = "abc";

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.QuantityOrdered);
            Assert.Equal(ValidationMessages.InvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── InvoicePricePaid value-is-zero / max value ──────────────────────────

        [Fact]
        public void InvoicePricePaid_WhenValueIsZeroError_ReturnsValueIsZeroError()
        {
            var row = ValidRow();
            row.InvoicePricePaidError = ErrorTypes.ValueIsZero;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.InvoicePricePaid);
            Assert.Equal(VirFileHeaders.InvoicePricePaid + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void InvoicePricePaid_WhenExceedsMaxValue_ReturnsWarning()
        {
            var row = ValidRow();
            row.InvoicePricePaid = (BigDecimal)5_000_001;
            row.InvoicePricePaidOriginalStr = "5000001";

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.InvoicePricePaid);
            Assert.Equal(ValidationMessages.InvoicePricePaidIsOverMaximum, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Warning, failure.ErrorCode);
        }

        [Fact]
        public void InvoicePricePaid_WhenNullAndInvalidFormatError_ReturnsValueIsZeroError()
        {
            var row = ValidRow();
            row.InvoicePricePaid = null;
            row.InvoicePricePaidError = ErrorTypes.InvalidFormat;
            row.InvoicePricePaidOriginalStr = "0";

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.InvoicePricePaid);
            Assert.Equal(VirFileHeaders.InvoicePricePaid + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── UnitPrice validation ────────────────────────────────────────────────

        [Theory]
        [InlineData(ErrorTypes.ValueIsZero)]
        [InlineData(ErrorTypes.NegativeValue)]
        public void UnitPrice_WhenValueIsZeroOrNegative_ReturnsValueIsZeroError(ErrorTypes errorType)
        {
            var row = ValidRow();
            row.UnitPriceError = errorType;

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.UnitPrice);
            Assert.Equal(VirFileHeaders.UnitPrice + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void UnitPrice_WhenNullAndInvalidFormatError_ReturnsValueIsZeroError()
        {
            var row = ValidRow();
            row.UnitPrice = null;
            row.UnitPriceError = ErrorTypes.InvalidFormat;
            row.UnitPriceOriginalStr = "0";

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.UnitPrice);
            Assert.Equal(VirFileHeaders.UnitPrice + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── Release validation ──────────────────────────────────────────────────

        [Fact]
        public void Release_WhenNullAndCharacterLimitExceeded_ReturnsError()
        {
            var row = ValidRow();
            row.Release = null;
            row.ReleaseError = ErrorTypes.CharacterLimitExceeded;
            row.ReleaseOriginalStr = new string('9', 51);

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.ReleaseNumber);
            Assert.Equal(ValidationMessages.CharacterLimitHasBeenExceeded, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        [Fact]
        public void Release_WhenNullAndInvalidFormatError_ReturnsValueIsZeroError()
        {
            var row = ValidRow();
            row.Release = null;
            row.ReleaseError = ErrorTypes.InvalidFormat;
            row.ReleaseOriginalStr = "0";

            var result = _validator.Validate(row);

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors, f => f.PropertyName == VirFileHeaders.ReleaseNumber);
            Assert.Equal(VirFileHeaders.ReleaseNumber + ValidationMessages.ValueIsZeroInvalidValue, failure.ErrorMessage);
            Assert.Equal(DataRowErrorStatus.Errors, failure.ErrorCode);
        }

        // ── IncorrectColumnCount ────────────────────────────────────────────────

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
    }
}