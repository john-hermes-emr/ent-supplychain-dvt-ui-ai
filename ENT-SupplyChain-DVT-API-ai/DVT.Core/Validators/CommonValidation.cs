using DVT.Core.Models;
using FluentValidation.Results;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    internal class CommonValidation
    {
        static string dash = "-";
        private static readonly Regex _asciiRegex = new Regex("^[\u0000-\u007F]*$", RegexOptions.Compiled);

        public static bool ValidASCIIEnglishCharacter(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return true;
            }
            var pattern = SpecialStringRegularExpression.ASCII;
            return _asciiRegex.IsMatch(str);
        }

        /// <summary>
        /// User Story 23111145: DVT - Expand Character Sets Validation for Selected Fields
        /// Bug 23714613: [QA Bug] - ITEM file - Part description - Error for Expand Character set validation (UTF-8)
        /// Bug 23714624: [QA Bug] - Supplier file - Supplier Name and Addr1-4 - Error for Expand Character set validation (UTF-8)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ValidUTF8Character(string str)
        {
            return true;

            /*
            if (string.IsNullOrWhiteSpace(str))
            {
                return true;
            }

            //After research, we found that the most straightforward way to check if a string contains only UTF-8 characters is to compare the byte count of the string when encoded in UTF-8 with the character count of the string. If they are equal, thus valid UTF-8 characters. 
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Utf8.IsValid(bytes);            
            */
        }

        public static bool ValidMainTelephone(string mainTelephone)
        {
            if (string.IsNullOrWhiteSpace(mainTelephone))
            {
                return true;
            }
            var pattern = SpecialStringRegularExpression.NumericWithDashes;
            return Regex.IsMatch(mainTelephone, pattern);
        }

        public static void ValidateHeaders(FileValidationResult fileResult, List<string> headers, List<string> expectedHeaders)
        {
            if (headers == null || headers.Count == 0)
            {
                fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                {
                     new ValidationFailure()
                     {
                        PropertyName = CustomFileHeaders.AllHeaderFields,
                        ErrorMessage = ValidationMessages.HeaderDoesNotMatchRequiredFormat,
                        ErrorCode = DataRowErrorStatus.Critical,
                        AttemptedValue = dash
                     }
                }), -1));
                //return false;
                return;
            }

            if (headers.Count != expectedHeaders.Count)
            {
                fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure()
                    {
                        PropertyName = ValidationMessages.FullRecordError,
                        ErrorMessage = ValidationMessages.HeaderDoesNotMatchRequiredFormat,
                        ErrorCode = DataRowErrorStatus.Critical,
                        AttemptedValue = dash
                    }
                }), 1));
                //return false;
                return;
            }

            var errors = new StringBuilder();
            for (int i = 0; i < expectedHeaders.Count; i++)
            {
                if (!string.Equals(headers[i], expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    errors.AppendLine(string.Format(ValidationMessages.HeaderMisMatchDetail, i + 1, expectedHeaders[i], headers[i]));
                }
            }

            if (errors.Length > 0)
            {
                fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure()
                    {
                        PropertyName = ValidationMessages.FullRecordError,
                        ErrorMessage = ValidationMessages.HeaderDoesNotMatchRequiredFormat,
                        ErrorCode = DataRowErrorStatus.Critical,
                        AttemptedValue = dash
                    }
                }), 1));
            }
        }
    }
}
