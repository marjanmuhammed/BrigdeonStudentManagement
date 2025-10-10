using Microsoft.AspNetCore.Http;

namespace Bridgeon.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
}