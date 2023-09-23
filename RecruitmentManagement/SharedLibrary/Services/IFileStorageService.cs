using Microsoft.AspNetCore.Http;

namespace SharedLibrary.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
}