namespace lineaEtica_Api.Services;

public interface IBlobService
{
    Task<string> SubirAsync(Stream contenido, string nombreBlob, string contentType);
    Task<(Stream stream, string contentType)> DescargarAsync(string nombreBlob);
    Task EliminarAsync(string nombreBlob);
}
