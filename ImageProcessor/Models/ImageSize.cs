using System.Text.Json.Serialization;

namespace ImageProcessor.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImageSize
{
    Phone,
    Tablet,
    Desktop
}