using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MovieService.Application;
using MovieService.Application.Common.Interfaces;

namespace MovieService.Infrastructure.Storage;
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly string? _accountName;
    private readonly string? _accountKey;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
        var containerName = Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER");

        _accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME");
        _accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY");

        var serviceClient = new BlobServiceClient(connectionString);
        _containerClient = serviceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }
    
    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        return GenerateSasUrl(fileName); 
    }
    
    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return;

        var uri = new Uri(fileUrl);
        var blobName = uri.Segments.Last(); 

        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }


    private string GenerateSasUrl(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(2)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(
            new StorageSharedKeyCredential(_accountName, _accountKey)
        ).ToString();

        return $"{blobClient.Uri}?{sasToken}";
    }
}