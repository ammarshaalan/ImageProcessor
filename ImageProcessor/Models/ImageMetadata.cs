using System.Text.Json.Serialization;

namespace ImageProcessor.Models;

public class ImageMetadata
{
    public string Id { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DateTime? DateTaken { get; set; }
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string OriginalPath { get; set; } = string.Empty;
    public string PhonePath { get; set; } = string.Empty;
    public string TabletPath { get; set; } = string.Empty;
    public string DesktopPath { get; set; } = string.Empty;
}
