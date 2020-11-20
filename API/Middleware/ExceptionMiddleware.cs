using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
  public class ExceptionMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
      _env = env;
      _logger = logger;
      _next = next;
    }

    //this is happening in the context of an http request
    //middleware has access to the http request that is coming in
    public async Task InvokeAsync(HttpContext context)
    {
        try {
            await _next.Invoke(context);
        }
        catch(Exception ex) {
            _logger.LogError(ex,ex.Message); //to the terminal

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = _env.IsDevelopment() 
                ? new ApiException(context.Response.StatusCode,ex.Message,ex.StackTrace?.ToString()) //dev
                : new ApiException(context.Response.StatusCode,"Internal Server Error");

            var options = new JsonSerializerOptions(){
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize<ApiException>(response, options);

            await context.Response.WriteAsync(json);
        }
        
    }
  }
}