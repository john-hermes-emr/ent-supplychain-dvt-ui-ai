using DVT.Api.Models;
using DVT.Core;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Data;
using System.Net;
using System.Text.Json;

namespace DVT.Api.CustomExceptionMiddleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public GlobalExceptionHandler()
        {
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken cancellationToken)
        {
            //Temporarily log to console until we implement something more robust
            var fullExceptionMessage = GetInnerExceptionMessages(ex);
            Console.WriteLine($"Exception: {fullExceptionMessage}");

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = Constants.StardardMessages.InternalServerError
            };

            switch (ex)
            {
                case BadHttpRequestException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = Constants.StardardMessages.BadRequest;
                    break;
                case InvalidOperationException:
                    errorResponse.StatusCode = 403;
                    errorResponse.Message = Constants.StardardMessages.InvalidOperation;
                    errorResponse.ExceptionMessage = ex.Message; //Constants.StardardMessages.ProtectedCannotBeDeleted;
                    break;
                case DuplicateNameException:
                    errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = Constants.StardardMessages.DuplicateValue;
                    errorResponse.ExceptionMessage = ex.Message;
                    break;
                case ValidationException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = Constants.StardardMessages.ValidationError;
                    errorResponse.ExceptionMessage = ex.Message;
                    break;
                default:
                    {
                        errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorResponse.Message = Constants.StardardMessages.InternalServerError;
                        break;
                    }
            }
            context.Response.StatusCode = errorResponse.StatusCode;

            var errorResponseJson = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(errorResponseJson, cancellationToken);

            return true;
        }


        private string GetInnerExceptionMessages(Exception ex)
        {
            var messages = new List<string>();
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            return string.Join(" -> ", messages);
        }

    }
}
