using ImageProcessor.Helpers;
using ImageProcessor.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ImageProcessor.Services;

public class ImageService : IImageService
{
    private readonly IExifService _exifService;
    private readonly IWebHostEnvironment _environment;
    private readonly string _storageBasePath;

    public ImageService(IExifService exifService, IWebHostEnvironment environment)
    {
        _exifService = exifService;
        _environment = environment;
        _storageBasePath = Path.Combine(environment.ContentRootPath, "storage");
        Directory.CreateDirectory(_storageBasePath);
    }

    public async Task<UploadImageResponse> UploadImageAsync(IFormFile file)
    {
        if (!ImageProcessingHelper.IsValidImageFile(file.FileName, file.Length))
        {
            throw new ArgumentException("Invalid file format or size");
        }

        var id = Guid.NewGuid().ToString("N");
        var imageDirectory = Path.Combine(_storageBasePath, id);
        Directory.CreateDirectory(imageDirectory);

        // Extract metadata
        using var stream = file.OpenReadStream();
        var metadata =  _exifService.ExtractMetadata(stream, file.FileName);
        metadata.Id = id;
        metadata.FileSize = file.Length;
        metadata.ContentType = file.ContentType;

        // Save original file
        var originalPath = Path.Combine(imageDirectory, "original" + Path.GetExtension(file.FileName));
        using (var fileStream = new FileStream(originalPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        metadata.OriginalPath = GetRelativePath(originalPath);

        // Process and save resized versions
        stream.Position = 0;
        var response = new UploadImageResponse
        {
            Id = id,
            OriginalFileName = file.FileName,
            ResizedImageUrls = new Dictionary<string, string>()
        };

        foreach (var size in ImageProcessingHelper.ImageSizes)
        {
            var resizedPath = Path.Combine(imageDirectory, $"{size.Key}.webp");
            using (var resizedStream = await ImageProcessingHelper.ConvertToWebpAndResizeAsync(stream, size.Value))
            using (var fileStream = new FileStream(resizedPath, FileMode.Create))
            {
                await resizedStream.CopyToAsync(fileStream);
            }

            switch (size.Key)
            {
                case "phone":
                    metadata.PhonePath = GetRelativePath(resizedPath);
                    break;
                case "tablet":
                    metadata.TabletPath = GetRelativePath(resizedPath);
                    break;
                case "desktop":
                    metadata.DesktopPath = GetRelativePath(resizedPath);
                    break;
            }

            response.ResizedImageUrls[size.Key] = $"/api/images/{id}/{size.Key}";
            stream.Position = 0;
        }

        // Save metadata
        var metadataPath = Path.Combine(imageDirectory, "metadata.json");
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));
        response.MetadataUrl = $"/api/images/{id}/metadata";

        return response;
    }

    public async Task<FileStreamResult> GetResizedImageAsync(string id, ImageSize size)
    {
        var sizeString = size.ToString().ToLowerInvariant();
        if (!ImageProcessingHelper.ImageSizes.ContainsKey(sizeString))
        {
            throw new ArgumentException("Invalid size parameter");
        }

        var imagePath = Path.Combine(_storageBasePath, id, $"{sizeString}.webp");
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException();
        }

        var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(stream, "image/webp");
    }

    public async Task<ImageMetadata> GetImageMetadataAsync(string id)
    {
        var metadataPath = Path.Combine(_storageBasePath, id, "metadata.json");
        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException();
        }

        var json = await File.ReadAllTextAsync(metadataPath);
        return JsonSerializer.Deserialize<ImageMetadata>(json) 
            ?? throw new InvalidOperationException("Failed to deserialize metadata");
    }

    public async Task<bool> DeleteImageAsync(string id)
    {
        var directory = Path.Combine(_storageBasePath, id);
        if (!Directory.Exists(directory))
        {
            return false;
        }

        Directory.Delete(directory, true);
        return true;
    }

    private string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(_environment.ContentRootPath, fullPath);
    }
}