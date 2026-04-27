using DVT.Api.Models;
using DVT.Core;
using DVT.Core.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;

namespace DVT.Api.CustomExceptionMiddleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ValidationException ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.BadRequest);
            }
            catch (AccessViolationException ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.InternalServerError);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex, HttpStatusCode code)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var currEx = ex;
            StringBuilder sb = new StringBuilder();
            while (currEx != null)
            {
                sb.AppendLine(currEx.Message);
                currEx = currEx.InnerException;
            }

            return context.Response.WriteAsync(new ErrorDetails
            {
                Path= context.Request.Path,
                Method= context.Request.Method,
                StatusCode = context.Response.StatusCode,
                Message = code.ToString(),
                ExceptionMessage = sb.ToString()
            }.ToString());
        }
    }
}
