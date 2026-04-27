using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Models
{
    public class FileRowValidationResult
    {
        public ValidationResult ValidationResult { get; set; } = new ValidationResult();
        public int RowNumber { get; set; }
        public bool IsValid
        {
            get { return ValidationResult.IsValid; }
        }

        //public string Status { get; set; } = DataRowErrorStatus.Errors;

        public FileRowValidationResult(ValidationResult result, int rowNumber)//, string status)
        {
            ValidationResult = result;
            RowNumber = rowNumber;
            //Status = status;
        }
    }
}
