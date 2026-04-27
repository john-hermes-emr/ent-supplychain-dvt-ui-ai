using System.Text.Json;

namespace DVT.Api.Models
{
    public class ErrorDetails
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }

        public List<ValidationErrorModel> ValidationErrors { get; set; } = new List<ValidationErrorModel>();

        public ErrorDetails(string validationFieldName, string validationMessage)
        {
            ValidationErrors.Add(new ValidationErrorModel
            {
                FieldName = validationFieldName,
                Message = validationMessage
            });
        }

        public ErrorDetails()
        {

        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
