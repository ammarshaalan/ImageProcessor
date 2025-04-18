using ImageProcessor.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ImageProcessor.Services;

public class ExifService : IExifService
{
    public ImageMetadata ExtractMetadata(Stream imageStream, string fileName)
    {
        var metadata = new ImageMetadata
        {
            OriginalFileName = fileName,
            UploadDate = DateTime.UtcNow
        };

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(imageStream);

            var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if (exifIfd0Directory != null)
            {
                metadata.CameraMake = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagMake);
                metadata.CameraModel = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagModel);
            }

            if (exifSubIfdDirectory != null && exifSubIfdDirectory.ContainsTag(ExifDirectoryBase.TagDateTimeOriginal))
            {
                var dateTaken = exifSubIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
                metadata.DateTaken = dateTaken;
            }

            if (gpsDirectory != null)
            {
                var geoLocation = gpsDirectory.GetGeoLocation();
                if (geoLocation != null)
                {
                    metadata.Latitude = geoLocation.Latitude;
                    metadata.Longitude = geoLocation.Longitude;
                }
            }
        }
        catch (Exception)
        {
            // Log the exception if needed, but continue without EXIF data
        }

        return metadata;
    }
}