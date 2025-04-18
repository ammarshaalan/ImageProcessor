using ImageProcessor.Models;
using ImageProcessor.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace ImageProcessorAPI.Tests.Services;

public class ImageServiceTests : IDisposable
{
    private readonly Mock<IExifService> _exifServiceMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<ImageService>> _loggerMock;
    private readonly ImageService _imageService;
    private readonly string _testStoragePath;
    private readonly string _testStorageBasePath;

    public ImageServiceTests()
    {
        _exifServiceMock = new Mock<IExifService>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<ImageService>>();

        // Create a unique test directory for each test run
        _testStoragePath = Path.Combine(Path.GetTempPath(), "ImageProcessorTest_" + Guid.NewGuid().ToString());
        _testStorageBasePath = Path.Combine(_testStoragePath, "storage");
        Directory.CreateDirectory(_testStorageBasePath);
        _environmentMock.Setup(e => e.ContentRootPath).Returns(_testStoragePath);

        _imageService = new ImageService(_exifServiceMock.Object, _environmentMock.Object);
    }

    public void Dispose()
    {
        // Clean up test directory after each test
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
    }

    [Fact]
    public async Task GetResizedImageAsync_InvalidSize_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var invalidSize = (ImageSize)999; // Invalid enum value

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _imageService.GetResizedImageAsync(id, invalidSize));
    }

    [Fact]
    public async Task GetResizedImageAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var size = ImageSize.Phone;

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _imageService.GetResizedImageAsync(id, size));
    }

    [Fact]
    public async Task GetImageMetadataAsync_ValidId_ReturnsMetadata()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var metadata = new ImageMetadata
        {
            Id = id,
            FileSize = 1024,
            ContentType = "image/jpeg"
        };
        var imageDir = Path.Combine(_testStorageBasePath, id);
        Directory.CreateDirectory(imageDir);
        var metadataPath = Path.Combine(imageDir, "metadata.json");
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));

        // Act
        var result = await _imageService.GetImageMetadataAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(1024, result.FileSize);
        Assert.Equal("image/jpeg", result.ContentType);
    }

    [Fact]
    public async Task GetImageMetadataAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _imageService.GetImageMetadataAsync(id));
    }

    [Fact]
    public async Task DeleteImageAsync_ExistingImage_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var imageDir = Path.Combine(_testStorageBasePath, id);
        Directory.CreateDirectory(imageDir);
        await File.WriteAllTextAsync(Path.Combine(imageDir, "test.txt"), "test content");

        // Act
        var result = await _imageService.DeleteImageAsync(id);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(imageDir));
    }

    [Fact]
    public async Task DeleteImageAsync_NonExistingImage_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();

        // Act
        var result = await _imageService.DeleteImageAsync(id);

        // Assert
        Assert.False(result);
    }
}