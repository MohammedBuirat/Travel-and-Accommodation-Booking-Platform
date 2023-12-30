using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/images")]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly IImageService _imageService;

        public ImageController(
            ILogger<ImageController> logger,
            IImageService imageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        [HttpGet("images/{path}")]
        public async Task<ActionResult> GetImage(string path)
        {
            try
            {
                var image = await _imageService.GetFileAsync(path);
                return File(image.Stream, image.ContentType, image.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching image: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("images")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateImage([FromBody] FileUpload image, string imagePath)
        {
            try
            {
                if (image == null || imagePath.IsNullOrEmpty())
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _imageService.DeleteFileAsync(imagePath);
                await _imageService.AddFileAsync(image, imagePath);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding a new hotel image: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
