using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.Review;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<ReviewService> _logger;
        private readonly IHotelService _hotelService;
        private readonly ReviewValidation _reviewValidation;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<Hotel> _hotelRepository;

        public ReviewService(IReviewRepository reviewRepository,
            ILogger<ReviewService> logger,
            IHotelService hotelService,
            ReviewValidation reviewValidation,
            IMapper mapper,
            UserManager<User> userManager,
            IRepository<Hotel> hotelRepository)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _reviewRepository = reviewRepository ??
                throw new ArgumentNullException(nameof(reviewRepository));
            _hotelService = hotelService ??
                throw new ArgumentNullException(nameof(hotelService));
            _reviewValidation = reviewValidation ??
                throw new ArgumentNullException(nameof(reviewValidation));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ??
                throw new ArgumentNullException(nameof(userManager));
            _hotelRepository = hotelRepository ??
                throw new ArgumentNullException(nameof(hotelRepository));
        }




        public async Task<IEnumerable<ReviewDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var pagingInfo = new Paging(pageSize, pageNumber);
                var reviews = await _reviewRepository.GetAllAsync(pagingInfo);
                var reviewsToReturn = _mapper.Map<List<ReviewDto>>(reviews);
                Logs<Review>.GetEntitiesLog(_logger, "Reviews");
                return reviewsToReturn;
            }
            catch (Exception ex)
            {
                Logs<Review>.GetEntitiesException(_logger, ex, "Reviews");
                throw;
            }
        }

        public async Task<ReviewDto> GetByIdAsync(Guid id)
        {
            try
            {
                var review = await GetReviewAsync(id);
                var reviewToReturn = _mapper.Map<ReviewDto>(review);
                Logs<Review>.GetEntityLog(_logger, "Review", id);
                return reviewToReturn;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<Review>.GetEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, ClaimsPrincipal user)
        {
            try
            {
                var review = await GetReviewAsync(id);
                AuthorizeUser(user, review.UserId);
                await _reviewRepository.DeleteAsync(id);
                await _hotelService.EditHotelTotalReviewsAsync(review.HotelId);
                Logs<Review>.DeleteEntityLog(_logger, "Review", review);
            }
            catch (Exception ex)
            {
                Logs<Review>.DeleteEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, ReviewDto reviewToUpdate, ClaimsPrincipal user)
        {
            try
            {
                var oldReview = await GetReviewAsync(id);
                var updatedReview = oldReview.Clone();
                _mapper.Map(reviewToUpdate, updatedReview);
                AuthorizeUser(user, updatedReview.UserId);
                await ValidateReviewAsync(updatedReview);
                await _reviewRepository.UpdateAsync(updatedReview);
                await _hotelService.EditHotelTotalReviewsAsync(updatedReview.HotelId);
                Logs<Review>.UpdateEntityLog(_logger, "Review", oldReview);
            }
            catch (Exception ex)
            {
                Logs<Review>.UpdateEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        public async Task<Review> AddAsync(ReviewToAdd review, ClaimsPrincipal user)
        {
            try
            {
                var reviewToBeAdded = _mapper.Map<Review>(review);
                reviewToBeAdded.DateOfReview = DateTime.Now;
                AuthorizeUser(user, reviewToBeAdded.UserId);
                await ValidateReviewAsync(reviewToBeAdded);
                var newReview = await _reviewRepository.AddAsync(reviewToBeAdded);
                await _hotelService.EditHotelTotalReviewsAsync(newReview.HotelId);
                Logs<Review>.AddEntityLog(_logger, "Review", reviewToBeAdded);
                return newReview;
            }
            catch (Exception ex)
            {
                Logs<Review>.AddEntityException(_logger, ex, "Review", _mapper.Map<Review>(review));
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Review> jsonPatchDocument, ClaimsPrincipal user)
        {
            try
            {
                var oldReview = await GetReviewAsync(id);
                AuthorizeUser(user, oldReview.UserId);
                var newReview = oldReview.Clone();
                jsonPatchDocument.ApplyTo(newReview);
                await ValidateReviewAsync(newReview);
                await _reviewRepository.UpdateAsync(newReview);
                await _hotelService.EditHotelTotalReviewsAsync(oldReview.HotelId);
                Logs<Review>.UpdateEntityLog(_logger, "Review", oldReview);
            }
            catch (Exception ex)
            {
                Logs<Review>.UpdateEntityException(_logger, ex, "Review", id);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetHotelReviewsAsync(Guid hotelId, int? pageSize, int? pageNumber)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(hotelId);
                if (hotel == null)
                {
                    throw new ElementNotFoundException();
                }

                var pagingInfo = new Paging(pageSize, pageNumber);
                Expression<Func<Review, bool>> filter = r => r.HotelId == hotelId;
                var customExpression = new CustomExpression<Review> { Filter = filter, Paging = pagingInfo };

                var reviews = await _reviewRepository.GetFilteredItemsAsync(customExpression);
                List<ReviewDto> reviewsToReturn = _mapper.Map<List<ReviewDto>>(reviews);
                return reviewsToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetHotelReviews method.");
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(string userId, int? pageSize, int? pageNumber)
        {
            try
            {
                bool userExists = (await _userManager.FindByIdAsync(userId) != null);
                if (!userExists)
                {
                    throw new ElementNotFoundException();
                }

                var pagingInfo = new Paging(pageSize, pageNumber);
                Expression<Func<Review, bool>> filter = r => r.UserId == userId;
                var customExpression = new CustomExpression<Review> { Filter = filter, Paging = pagingInfo };

                var reviews = await _reviewRepository.GetFilteredItemsAsync(customExpression);
                List<ReviewDto> reviewsToReturn = _mapper.Map<List<ReviewDto>>(reviews);
                return reviewsToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetUserReviews method.");
                throw;
            }
        }

        public async Task<HotelReviewSummary> GetHotelReviewsSummaryAsync(Guid hotelId)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(hotelId);
                if (hotel == null)
                {
                    throw new ElementNotFoundException();
                }
                var reviewSummary = await _reviewRepository.GetHotelReviewSummaryAsync(hotelId);
                return reviewSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving hotel {hoteId} review summary");
                throw;
            }
        }
        private async Task<Review> GetReviewAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                throw new ElementNotFoundException();
            }
            return review;
        }

        private async Task ValidateReviewAsync(Review entity)
        {
            var validationResults = await _reviewValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }

        private void AuthorizeUser(ClaimsPrincipal userClaims, string id)
        {
            var userId = userClaims.Claims.FirstOrDefault(c => c.Type == "Sub")?.Value;
            var userRole = userClaims.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "User";
            if (userId != id && userRole != "Admin")
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
