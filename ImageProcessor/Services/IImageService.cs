using ImageProcessor.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Services;

public interface IImageService
{
    Task<UploadImageResponse> UploadImageAsync(IFormFile file);
    Task<FileStreamResult> GetResizedImageAsync(string id, ImageSize size);
    Task<ImageMetadata> GetImageMetadataAsync(string id);
    Task<bool> DeleteImageAsync(string id);
} 