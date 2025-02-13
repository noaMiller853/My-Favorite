using WebApplicationUser.Models.WebApplicationUser.Models;

namespace WebApplicationUser.Interfaces
{
    public interface IGoogleDrive
    {
        Task<string> UploadFileToDrive(Stream fileStream, string fileName, string contentType);
        Task DeleteFileFromDrive(string fileId);
        Task<Stream> DownloadFileFromDrive(int fileId);
    }
}
