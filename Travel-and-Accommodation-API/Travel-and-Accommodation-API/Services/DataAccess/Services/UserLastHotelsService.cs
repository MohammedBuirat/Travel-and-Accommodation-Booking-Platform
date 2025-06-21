using AutoMapper;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.UserLastHotels;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class UserLastHotelsService : IUserLastHotelsService
    {
        private readonly IRepository<UserLastHotels> _repository;
        private readonly ILogger<UserLastHotelsService> _logger;
        private readonly IMapper _mapper;
        const int MaxUserLastHotels = 5;
        public UserLastHotelsService(IRepository<UserLastHotels> repository,
            ILogger<UserLastHotelsService> logger,
            IMapper mapper)
        {
            _repository = repository ??
                throw new ArgumentNullException(nameof(_repository));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<UserLastHotels> AddAsync(UserLastHotels entity)
        {
            try
            {
                entity.DateOfView = DateTime.Now;
                Expression<Func<UserLastHotels, bool>> filter = uh => uh.UserId == entity.UserId;
                var ex = new CustomExpression<UserLastHotels>();
                ex.Filter = filter;
                var userLastHotels = await _repository.GetFilteredItemsAsync(ex);
                var hotelWithTheEntityId = userLastHotels.Where(uh => uh.HotelId == entity.HotelId).FirstOrDefault();
                if (hotelWithTheEntityId != null)
                {
                    return null;
                }
                if (userLastHotels == null || userLastHotels.Count() < MaxUserLastHotels)
                {
                    Logs<UserLastHotels>.AddEntityLog(_logger, "UserLastHotels", entity);
                    return await _repository.AddAsync(entity);
                }
                else
                {
                    var lastHotel = userLastHotels.OrderBy(lh => lh.DateOfView).FirstOrDefault();
                    Expression<Func<UserLastHotels, bool>> deleteExpression = uh => uh.HotelId == lastHotel.HotelId &&
                    uh.UserId == lastHotel.UserId;
                    await _repository.DeleteAsync(deleteExpression);
                    Logs<UserLastHotels>.AddEntityLog(_logger, "UserLastHotels", entity);
                    return await _repository.AddAsync(entity);
                }
            }
            catch (Exception ex)
            {
                Logs<UserLastHotels>.AddEntityException(_logger, ex, "UserLastHotels", entity);
                throw;
            }
        }

        public async Task<IEnumerable<UserLastHotelsDto>> GetUsersLastHotelsAsync(string userId, ClaimsPrincipal user)
        {
            try
            {
                AuthorizeUser(user, userId);
                Expression<Func<UserLastHotels, bool>> filter = ulh => ulh.UserId == userId;
                var expression = new CustomExpression<UserLastHotels>();
                expression.Filter = filter;

                var userLastHotels = await _repository.GetFilteredItemsAsync(expression);
                var userLastHotelsDto = _mapper.Map<List<UserLastHotelsDto>>(userLastHotels);

                return userLastHotelsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request in GetUserLastHotels method.");
                throw;
            }
        }

        public async Task DeleteAsync(Guid hotelId, string userId, ClaimsPrincipal user)
        {
            try
            {
                AuthorizeUser(user, userId);
                UserLastHotels? userLastHotels = await _repository.GetFirstOrDefaultAsync(ulh => ulh.HotelId == hotelId && ulh.UserId == userId);
                if (userLastHotels == null)
                {
                    throw new ElementNotFoundException();
                }
                Logs<UserLastHotels>.DeleteEntityLog(_logger, "UserLastHotels", userLastHotels);

                await _repository.DeleteAsync(ulh => ulh.HotelId == hotelId && ulh.UserId == userId);
            }
            catch (Exception ex)
            {
                Logs<UserLastHotels>.DeleteEntityException(_logger, ex, "UserLastHotels", Guid.Parse(userId), hotelId);
                throw;
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
