using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware (RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception e)
        {
          logger.LogError(e, e.Message);
          httpContext.Response.ContentType = "application/json";
          httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
          
          var response = env.IsDevelopment() 
              ? new ApiExceptions (httpContext.Response.StatusCode, e.Message, e.StackTrace) 
              : new ApiExceptions (httpContext.Response.StatusCode, e.Message, "Internal Server error");

          var options = new JsonSerializerOptions
          {
              PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
          };
          
          var json = JsonSerializer.Serialize(response, options);
          
          await httpContext.Response.WriteAsync(json);
        }
    }
}