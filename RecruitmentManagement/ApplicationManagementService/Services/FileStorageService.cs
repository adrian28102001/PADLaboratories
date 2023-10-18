namespace ApplicationManagementService.Services
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            var filePath = "";
            try
            {
                // Use Path.Combine to create paths and let it decide the appropriate directory separator.
                var directoryPath = Path.Combine("RecruitmentManagement", "FileStorage");

                // Ensure the directory exists before saving the file.
                Directory.CreateDirectory(directoryPath);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                filePath = Path.Combine(directoryPath, uniqueFileName);

                await using var stream = File.Create(filePath);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed save pdf file. Exception: {ex.Message}");
            }

            return filePath;
        }
    }
}