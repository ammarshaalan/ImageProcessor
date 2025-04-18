using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ImageProcessor.HealthChecks;

public class ImageServiceHealthCheck : IHealthCheck
{
    private readonly IWebHostEnvironment _environment;

    public ImageServiceHealthCheck(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var storagePath = Path.Combine(_environment.ContentRootPath, "storage");

            // Check if storage directory exists and is accessible
            if (!Directory.Exists(storagePath))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Storage directory does not exist"));
            }

            // Check if we can write to the storage directory
            var testFile = Path.Combine(storagePath, "healthcheck.tmp");
            File.WriteAllText(testFile, DateTime.UtcNow.ToString());
            File.Delete(testFile);

            // Get available disk space
            var driveInfo = new DriveInfo(Path.GetPathRoot(storagePath)!);
            var freeSpace = driveInfo.AvailableFreeSpace;
            var totalSpace = driveInfo.TotalSize;
            var freeSpacePercentage = (double)freeSpace / totalSpace * 100;

            var data = new Dictionary<string, object>
            {
                { "StoragePath", storagePath },
                { "FreeSpace", freeSpace },
                { "TotalSpace", totalSpace },
                { "FreeSpacePercentage", freeSpacePercentage }
            };

            return Task.FromResult(
                freeSpacePercentage < 10
                    ? HealthCheckResult.Degraded("Low disk space", data: data)
                    : HealthCheckResult.Healthy("Storage is healthy", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage check failed", ex));
        }
    }
}