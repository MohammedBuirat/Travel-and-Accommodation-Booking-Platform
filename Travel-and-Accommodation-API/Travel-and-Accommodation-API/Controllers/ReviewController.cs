using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Review;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;


        public ReviewController(IReviewService reviewService,
            ILogger<ReviewController> logger)
        {
            _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(int? pageSize, int? pageNumber)
        {
            try
            {
                var reviews = await _reviewService.GetAllAsync(pageSize, pageNumber);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                Logs<Review>.GetEntitiesException(_logger, ex, "Reviews");
                throw;
            }
        }

        [HttpGet("reviews/{id}")]
        public async Task<ActionResult<ReviewDto>> GetReviewById(Guid id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
                return Ok(review);
            }
            catch (Exception ex)
            {
                Logs<Review>.GetEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        [HttpDelete("reviews/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteReview(Guid id)
        {
            try
            {
                await _reviewService.DeleteAsync(id, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Review>.DeleteEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        [HttpPut("reviews/{id}")]
        public async Task<ActionResult> UpdateReview(Guid id, [FromBody] ReviewDto reviewToUpdate)
        {
            try
            {
                if (reviewToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                await _reviewService.UpdateAsync(id, reviewToUpdate, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Review>.DeleteEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        [HttpPost("reviews")]
        public async Task<ActionResult> AddReview([FromBody] ReviewToAdd review)
        {
            try
            {
                if (review == null)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var newReview = await _reviewService.AddAsync(review, User);
                return CreatedAtAction(nameof(GetReviewById), new { id = newReview.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<ReviewToAdd>.AddEntityException(_logger, ex, "Review");
                throw;
            }
        }

        [HttpPatch("reviews/{id}")]
        public async Task<ActionResult> PartialUpdateReview(Guid id, JsonPatchDocument<Review> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    return BadRequest();
                }

                await _reviewService.PartialUpdateAsync(id, jsonPatchDocument, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Review>.UpdateEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        [HttpGet("hotels/{hotelId}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetHotelReviews(Guid hotelId, int? pageSize, int? pageNumber)
        {
            try
            {
                var reviews = await _reviewService.GetHotelReviewsAsync(hotelId, pageSize, pageNumber);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetHotelReviews method.");
                throw;
            }
        }

        [HttpGet("users/{userId}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetUserReviews(string userId, int? pageSize, int? pageNumber)
        {
            try
            {
                var reviews = await _reviewService.GetUserReviewsAsync(userId, pageSize, pageNumber);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetUserReviews method.");
                throw;
            }
        }

        [HttpGet("hotels/{hotelId}/reviewSummary")]
        public async Task<ActionResult<HotelReviewSummary>> GetHotelReviewsSummary(Guid hotelId)
        {
            try
            {
                var hotelReviewsSummary = await _reviewService.GetHotelReviewsSummaryAsync(hotelId);
                return Ok(hotelReviewsSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving hotel {hotelId} reviews summary");
                throw;
            }
        }
    }
}
