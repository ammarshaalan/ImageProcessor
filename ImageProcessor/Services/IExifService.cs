using ImageProcessor.Models;

namespace ImageProcessor.Services;

public interface IExifService
{
    ImageMetadata ExtractMetadata(Stream imageStream, string fileName);
}