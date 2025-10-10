using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Bridgeon.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _folder;

        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
            _folder = configuration["Cloudinary:Folder"]; // ✅ get folder from appsettings
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Validate file type
            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type");


            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size too large");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = _folder, // ✅ save inside configured folder
                Transformation = new Transformation()
                    .Width(500).Height(500).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}
