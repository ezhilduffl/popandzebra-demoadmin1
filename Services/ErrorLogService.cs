using PopZebra.Data;
using PopZebra.Models;

namespace PopZebra.Services
{
    public class ErrorLogService
    {
        private readonly AppDbContext _db;

        public ErrorLogService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(Exception ex, HttpContext? httpContext = null)
        {
            try
            {
                // ── Extract first line from stack trace ───────
                string? errorLine = null;
                if (ex.StackTrace != null)
                {
                    var firstLine = ex.StackTrace
                        .Split('\n')
                        .FirstOrDefault(l => l.Trim().StartsWith("at "));

                    if (firstLine != null)
                        errorLine = firstLine.Trim().Length > 500
                            ? firstLine.Trim()[..500]
                            : firstLine.Trim();
                }

                // ── Get page name from request ────────────────
                string? pageName = httpContext?.Request.Path.Value;

                _db.ErrorLogs.Add(new ErrorLog
                {
                    ErrorMessage = ex.Message,
                    PageName = pageName,
                    ErrorLine = errorLine,
                    AddedDate = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
            }
            catch
            {
                // Silently fail — never crash the app
                // while trying to log an error
            }
        }
    }
}