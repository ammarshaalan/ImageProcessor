using ImageProcessor.Controllers;
using ImageProcessor.Models;
using ImageProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ImageProcessorAPI.Tests.Controllers;

public class ImageControllerTests
{
    private readonly Mock<IImageService> _imageServiceMock;
    private readonly Mock<ILogger<ImageController>> _loggerMock;
    private readonly ImageController _controller;

    public ImageControllerTests()
    {
        _imageServiceMock = new Mock<IImageService>();
        _loggerMock = new Mock<ILogger<ImageController>>();
        _controller = new ImageController(_imageServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetResizedImageAsync_ValidParameters_ReturnsFileStreamResult()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var size = ImageSize.Phone;
        var expectedResult = new FileStreamResult(new MemoryStream(), "image/webp");
        _imageServiceMock.Setup(s => s.GetResizedImageAsync(id, size))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetResizedImage(id, size);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        _imageServiceMock.Verify(s => s.GetResizedImageAsync(id, size), Times.Once);
    }

    [Fact]
    public async Task GetResizedImageAsync_InvalidSize_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var size = ImageSize.Phone;
        _imageServiceMock.Setup(s => s.GetResizedImageAsync(id, size))
            .ThrowsAsync(new ArgumentException("Invalid size parameter"));

        // Act
        var result = await _controller.GetResizedImage(id, size);

        // Assert
        Assert.NotNull(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid size parameter", badRequestResult.Value);
    }

    [Fact]
    public async Task GetResizedImageAsync_FileNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var size = ImageSize.Phone;
        _imageServiceMock.Setup(s => s.GetResizedImageAsync(id, size))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        var result = await _controller.GetResizedImage(id, size);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetImageMetadata_ValidId_ReturnsMetadata()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var expectedMetadata = new ImageMetadata
        {
            Id = id,
            FileSize = 1024,
            ContentType = "image/jpeg"
        };
        _imageServiceMock.Setup(s => s.GetImageMetadataAsync(id))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _controller.GetImageMetadata(id);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var metadata = Assert.IsType<ImageMetadata>(okResult.Value);
        Assert.Equal(id, metadata.Id);
        Assert.Equal(1024, metadata.FileSize);
        Assert.Equal("image/jpeg", metadata.ContentType);
    }

    [Fact]
    public async Task GetImageMetadata_FileNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _imageServiceMock.Setup(s => s.GetImageMetadataAsync(id))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        var result = await _controller.GetImageMetadata(id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteImage_ExistingImage_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _imageServiceMock.Setup(s => s.DeleteImageAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteImage(id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteImage_NonExistingImage_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _imageServiceMock.Setup(s => s.DeleteImageAsync(id))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteImage(id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }
}