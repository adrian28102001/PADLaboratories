namespace ApplicationManagementService.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
}