using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace EventPulse.BLL.Services;

public class ImageService : IImageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;
    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(byte[] imageBytes, string fileName, string subfolder)
    {
        string extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
            throw new BadRequestException($"File extension '{extension}' is not allowed. Allowed: .jpg, .jpeg, .png");

        if (imageBytes.Length > MaxFileSize)
            throw new BadRequestException("File size exceeds the maximum allowed size of 5MB.");

        string relativeFolder = Path.Combine("uploads", subfolder);
        string folderPath = Path.Combine(_environment.WebRootPath, relativeFolder);
        Directory.CreateDirectory(folderPath);

        string uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string filePath = Path.Combine(folderPath, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, imageBytes);

        return Path.Combine(relativeFolder, uniqueFileName).Replace("\\", "/");
    }

    public void DeleteImage(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        string fullPath = Path.Combine(_environment.WebRootPath, relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
