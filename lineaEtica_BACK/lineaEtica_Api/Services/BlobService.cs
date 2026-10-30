using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace lineaEtica_Api.Services;

public class BlobService : IBlobService
{
    private readonly BlobContainerClient _container;

    public BlobService(IConfiguration config)
    {
        var connStr        = config["AzureStorage:ConnectionString"]
                             ?? throw new InvalidOperationException("AzureStorage:ConnectionString no configurado.");
        var containerName  = config["AzureStorage:ContainerName"] ?? "investigacion-adjuntos";
        _container = new BlobContainerClient(connStr, containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> SubirAsync(Stream contenido, string nombreBlob, string contentType)
    {
        var blob = _container.GetBlobClient(nombreBlob);
        await blob.UploadAsync(contenido, new BlobHttpHeaders { ContentType = contentType });
        return nombreBlob;
    }

    public async Task<(Stream stream, string contentType)> DescargarAsync(string nombreBlob)
    {
        var blob        = _container.GetBlobClient(nombreBlob);
        var response    = await blob.DownloadStreamingAsync();
        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
        var ms          = new MemoryStream();
        await response.Value.Content.CopyToAsync(ms);
        ms.Position = 0;
        return (ms, contentType);
    }

    public async Task EliminarAsync(string nombreBlob)
    {
        var blob = _container.GetBlobClient(nombreBlob);
        await blob.DeleteIfExistsAsync();
    }
}
