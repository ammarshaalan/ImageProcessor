namespace ImageProcessor.Models;

public class UploadImageResponse
{
    public string Id { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public Dictionary<string, string> ResizedImageUrls { get; set; } = new();
    public string MetadataUrl { get; set; } = string.Empty;
} 