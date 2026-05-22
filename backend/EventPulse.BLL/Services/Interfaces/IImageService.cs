namespace EventPulse.BLL.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(byte[] imageBytes, string fileName, string subfolder);
    void DeleteImage(string relativePath);
}
