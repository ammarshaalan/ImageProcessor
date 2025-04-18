using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Helpers;

public static class ImageProcessingHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public static readonly Dictionary<string, Size> ImageSizes = new()
    {
        { "phone", new Size(640, 480) },
        { "tablet", new Size(1024, 768) },
        { "desktop", new Size(1920, 1080) }
    };

    public static bool IsValidImageFile(string fileName, long fileSize)
    {
        if (fileSize > MaxFileSize)
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    public static async Task<Stream> ConvertToWebpAndResizeAsync(Stream inputStream, Size targetSize)
    {
        inputStream.Position = 0;
        using var image = await Image.LoadAsync(inputStream);
        
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = targetSize,
            Mode = ResizeMode.Max
        }));

        var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream);
        outputStream.Position = 0;
        return outputStream;
    }
} 