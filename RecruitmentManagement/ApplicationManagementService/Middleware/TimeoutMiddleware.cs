using System.Net;

namespace ApplicationManagementService.Middleware;

public class TimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeSpan _timeout;

    public TimeoutMiddleware(RequestDelegate next, TimeSpan timeout)
    {
        _next = next;
        _timeout = timeout;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var cts = new CancellationTokenSource(_timeout);
        var timeoutTask = Task.Delay(_timeout, cts.Token);
        var nextTask = _next(context);

        var completedTask = await Task.WhenAny(timeoutTask, nextTask);
        if (completedTask == timeoutTask)
        {
            context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            await context.Response.WriteAsync("Request timeout", cancellationToken: cts.Token);
            cts.Cancel();
        }
    }
}
