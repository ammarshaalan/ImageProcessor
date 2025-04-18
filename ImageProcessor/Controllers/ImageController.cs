using ImageProcessor.Models;
using ImageProcessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit for multiple images
    public async Task<ActionResult<UploadImageResponse>> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var response = await _imageService.UploadImageAsync(file);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image upload");
            return StatusCode(500, "An error occurred while processing the image");
        }
    }

    [HttpGet("{id}/{size}")]
    public async Task<IActionResult> GetResizedImage(string id, [FromRoute] ImageSize size)
    {
        try
        {
            var result = await _imageService.GetResizedImageAsync(id, size);
            return result;
        }
        catch (ArgumentException)
        {
            return BadRequest("Invalid size parameter");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resized image");
            return StatusCode(500, "An error occurred while retrieving the image");
        }
    }

    [HttpGet("{id}/metadata")]
    public async Task<ActionResult<ImageMetadata>> GetImageMetadata(string id)
    {
        try
        {
            var metadata = await _imageService.GetImageMetadataAsync(id);
            return Ok(metadata);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image metadata");
            return StatusCode(500, "An error occurred while retrieving the metadata");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(string id)
    {
        try
        {
            var result = await _imageService.DeleteImageAsync(id);
            return result ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image");
            return StatusCode(500, "An error occurred while deleting the image");
        }
    }
} 