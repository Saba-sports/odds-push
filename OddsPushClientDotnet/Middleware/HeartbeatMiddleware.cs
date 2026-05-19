using System.Net;
using OddsPushClient.Services;

namespace OddsPushClient.Middleware;

public class HeartbeatMiddleware
{
    private readonly RequestDelegate _next;

    public HeartbeatMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHeartbeatMonitor heartbeatMonitor)
    {
        // Only check API requests
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            if (!heartbeatMonitor.IsServiceAvailable())
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                // Middleware runs after UseCors, so headers should be preserved if correctly configured.
                // However, we set them here too as a safety measure for short-circuiting.
                context.Response.Headers.AccessControlAllowOrigin = "*";
                context.Response.Headers.AccessControlAllowMethods = "*";
                context.Response.Headers.AccessControlAllowHeaders = "*";

                await context.Response.WriteAsync("Service is under maintenance (Heartbeat lost).");
                return;
            }
        }

        await _next(context);
    }
}

public static class HeartbeatMiddlewareExtensions
{
    public static IApplicationBuilder UseHeartbeatCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HeartbeatMiddleware>();
    }
}
