namespace MovieService.Application;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);
    Task DeleteFileAsync(string fileUrl);

}
