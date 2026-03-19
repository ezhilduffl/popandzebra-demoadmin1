using PopZebra.Services;

namespace PopZebra.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception at {Path}",
                    context.Request.Path);

                // ── Save to DB silently ───────────────────────
                try
                {
                    var errorLogService = context.RequestServices
                        .GetRequiredService<ErrorLogService>();
                    await errorLogService.LogAsync(ex, context);
                }
                catch
                {
                    // Never crash while logging
                }

                // ── Redirect to error page ────────────────────
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Error");
                }
            }
        }
    }
}