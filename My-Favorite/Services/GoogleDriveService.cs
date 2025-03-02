using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using NuGet.Common;
using WebApplicationUser.Interfaces;
using WebApplicationUser.Models.WebApplicationUser.Models;

public class GoogleDriveService:IGoogleDrive
{
    private readonly DriveService _driveService;
    private readonly string _folderId;

    public GoogleDriveService(IConfiguration configuration)
    {
        var credentialsPath = configuration["GoogleDrive:CredentialsPath"]
            ?? throw new ArgumentNullException("GoogleDrive CredentialsPath is missing");
        _folderId = configuration["GoogleDrive:FolderId"]
            ?? throw new ArgumentNullException("GoogleDrive FolderId is missing");

        var credential = GetGoogleCredential(credentialsPath);
        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "DocumentUploader"
        });
    }

    private GoogleCredential GetGoogleCredential(string credentialsPath)
    {
        using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
        return GoogleCredential.FromStream(stream)
            .CreateScoped(DriveService.Scope.DriveFile);
    }

    public async Task<string> UploadFileToDrive(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { _folderId }
            };

            var request = _driveService.Files.Create(fileMetadata, fileStream, contentType);
            request.Fields = "id";

            var uploadProgress = await request.UploadAsync();
            if (uploadProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                throw new Exception($"Upload failed: {uploadProgress.Exception.Message}");
            }

            return request.ResponseBody?.Id
                ?? throw new Exception("File uploaded but ID not received");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload file to Google Drive: {ex.Message}", ex);
        }
    }
    public async Task DeleteFileFromDrive(string fileId)
    {
        try
        {
            await _driveService.Files.Delete(fileId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete file from Google Drive: {ex.Message}", ex);
        }
    }
    public async Task<Stream> DownloadFileFromDrive(int fileId)
    {
        try
        {
            var request = _driveService.Files.Get(fileId.ToString());
            var stream = new MemoryStream();
            await request.DownloadAsync(stream);
            stream.Position = 0;
            return stream;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download file from Google Drive: {ex.Message}", ex);
        }
    }


    //public async IAsyncEnumerable<DocumentMeta> GetDocumentsAsync()
    //{
    //    var request = _driveService.Files.List();
    //    request.Q = $"'{_folderId}' in parents";
    //    request.Fields = "files(id, name, size, createdTime, mimeType)";

    //    // ביצוע הבקשה וקבלת התוצאה
    //    var response = await request.ExecuteAsync().ConfigureAwait(false);

    //    if (response?.Files is null || response.Files.Count == 0)
    //    {
    //        yield break; // החזרת אוסף ריק במקרה שאין קבצים
    //    }

    //    foreach (var file in response.Files)
    //    {
    //        yield return new DocumentMeta
    //        {
    //            FileName = file.Name,
    //            FileSize = file.Size ?? 0,
    //            UploadedAt = file.CreatedTime ?? DateTime.MinValue,
    //            FilePath = file.Id,
    //            ContentType = file.MimeType
    //        };
    //    }
    //}

}