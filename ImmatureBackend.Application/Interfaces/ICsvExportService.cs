namespace ImmatureBackend.Application.Interfaces;

public interface ICsvExportService
{
    Task<byte[]> ExportReplicatesAsync(CancellationToken cancellationToken = default);
}