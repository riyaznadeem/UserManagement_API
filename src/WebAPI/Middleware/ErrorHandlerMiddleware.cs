using Serilog;
using System.Net;
using System.Text.Json;

namespace WebAPI.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        #region IP Address
        var currentRequest = context.Request;
        var ip = currentRequest.Headers["X-Forwarded-For"].FirstOrDefault();
        string ipAddress = context.Connection.RemoteIpAddress.ToString();
        ipAddress = ipAddress + " / " + ip;
        #endregion

        try
        {
            Log.Information($"[{currentRequest.Path}] => API Access From IP MW: [{ipAddress}]");
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error($"[{currentRequest.Path}] => API Access From IP: {ipAddress} , Exception : [{ex.StackTrace}]");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Something went wrong",
                details = ex.Message
            }));
        }
    }
}
