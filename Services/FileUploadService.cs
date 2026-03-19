using Microsoft.AspNetCore.Http;

namespace PopZebra.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif" };

        private const long MaxFileSizeBytes = 3 * 1024 * 1024;

        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(bool Success, string? FilePath, string Error)>
            SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return (false, null, "No file selected.");

            if (file.Length > MaxFileSizeBytes)
                return (false, null, "File size must not exceed 3 MB.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                return (false, null,
                    "Only JPG, PNG, and GIF files are allowed.");

            var uploadsFolder = Path.Combine(
                _env.WebRootPath, "uploads", folder);

            try
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            catch (Exception ex)
            {
                return (false, null,
                    $"Cannot create upload folder. " +
                    $"Please check server permissions. ({ex.Message})");
            }

            var uniqueFileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return (false, null,
                    "Server does not have permission to save files. " +
                    "Please contact your hosting provider to grant " +
                    "write access to the uploads folder.");
            }
            catch (Exception ex)
            {
                return (false, null,
                    $"File could not be saved: {ex.Message}");
            }

            return (true, $"/uploads/{folder}/{uniqueFileName}", null);
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            try
            {
                var fullPath = Path.Combine(
                    _env.WebRootPath,
                    relativePath.TrimStart('/')
                                .Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch
            {
                // Silently fail on delete
            }
        }
    }
}