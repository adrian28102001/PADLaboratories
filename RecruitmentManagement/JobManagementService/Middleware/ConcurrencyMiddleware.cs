using JobManagementService.Configurations;

namespace JobManagementService.Middleware;

public class ConcurrencyMiddleware
{
    private readonly RequestDelegate _next;

    public ConcurrencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool entered = false;

        try
        {
            entered = await SemaphoreConfiguration.ConcurrencySemaphore.WaitAsync(0); // non-blocking

            if (!entered)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                return;
            }

            await _next(context);
        }
        finally
        {
            if (entered)
                SemaphoreConfiguration.ConcurrencySemaphore.Release();
        }
    }
}
