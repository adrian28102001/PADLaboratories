using SharedLibrary.Services;

namespace ApplicationManagementService.Services;

public class FileStorageService : IFileStorageService
{
    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var filePath = "";
        try
        {
            var directoryPath = "U:\\FourthYear\\Sem1\\PAD\\Labs\\Lab1\\RecruitmentManagement\\FileStorage";
            filePath = Path.Combine(directoryPath, file.FileName);
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