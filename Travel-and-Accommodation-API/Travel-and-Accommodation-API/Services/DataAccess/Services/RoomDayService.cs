using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.Dto.RoomDay;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class RoomDayService : IRoomDayService
    {
        private readonly IRoomDayRepository _roomDayRepository;
        private readonly ILogger<RoomDayService> _logger;
        private readonly IRepository<Room> _roomRepository;
        private readonly IMapper _mapper;
        private readonly RoomDayValidation _roomDayValidation;

        public RoomDayService(IRoomDayRepository roomDayRepository,
            ILogger<RoomDayService> logger,
            IRepository<Room> roomRepository,
            IMapper mapper,
            RoomDayValidation roomDayValidation)
        {
            _roomDayRepository = roomDayRepository ??
                throw new ArgumentNullException(nameof(roomDayRepository));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _roomRepository = roomRepository ??
                throw new ArgumentNullException(nameof(roomRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _roomDayValidation = roomDayValidation ??
                throw new ArgumentNullException(nameof(roomDayValidation));
        }

        public async Task InsertRoomDaysAsync(Room room)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                DateTime lastDate = currentDate.AddMonths(1);
                var roomDays = new List<RoomDay>();
                while (currentDate < lastDate)
                {
                    var roomDay = new RoomDay
                    {
                        Price = room.BasePrice,
                        RoomId = room.Id,
                        Available = true,
                        Date = currentDate
                    };
                    roomDays.Add(roomDay);
                    currentDate = currentDate.AddDays(1);
                }
                await _roomDayRepository.AddAllAsync(roomDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while inserting roomdays");
            }
        }

        public async Task<RoomDayDto> GetAsync(Guid roomId, string date)
        {
            try
            {
                var roomDay = await GetRoomDayAsync(roomId, date);

                RoomDayDto roomDayToReturn = _mapper.Map<RoomDayDto>(roomDay);
                return roomDayToReturn;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetRoomDay method.");
                throw;
            }
        }

        public async Task UpdateAsync(Guid roomId, string date, [FromBody] RoomDayDto roomDayToUpdate)
        {
            try
            {
                var oldRoomDay = await GetRoomDayAsync(roomId, date);
                var newRoomDay = oldRoomDay.Clone();
                _mapper.Map(roomDayToUpdate, newRoomDay);
                await ValidateRoomDayAsync(newRoomDay);

                await _roomDayRepository.UpdateAsync(newRoomDay);
                _logger.LogInformation($"Successfully updated RoomDay for Room ID: {roomId} on date: {date}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in UpdateRoomDay method.");
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid roomId, string date, JsonPatchDocument<RoomDay> jsonPatchDocument)
        {
            try
            {
                var oldRoomDay = await GetRoomDayAsync(roomId, date);
                var newRoomDay = oldRoomDay.Clone();
                jsonPatchDocument.ApplyTo(newRoomDay);
                await ValidateRoomDayAsync(newRoomDay);
                await _roomDayRepository.UpdateAsync(newRoomDay);
                _logger.LogInformation($"Successfully partially updated RoomDay for Room ID: {roomId} on date: {date}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in PartialUpdateRoomDay method.");
                throw;
            }
        }

        public async Task<IEnumerable<RoomDayDto>> GetRoomDaysForRoomAsync(Guid roomId, string? beginDate, string? endDate, bool? available)
        {
            try
            {
                var room = await _roomRepository.GetByIdAsync(roomId);
                if (room == null)
                {
                    throw new ElementNotFoundException();
                }

                var expressions = new List<Expression<Func<RoomDay, bool>>>();
                var customExpression = new CustomExpression<RoomDay>();
                Expression<Func<RoomDay, bool>> roomFilter = roomDay => roomDay.RoomId == roomId;
                expressions.Add(roomFilter);

                if (beginDate != null)
                {
                    DateTime startDate = DateTime.Parse(beginDate);
                    Expression<Func<RoomDay, bool>> startDateFilter = roomDay => roomDay.Date.Date >= startDate.Date;
                    expressions.Add(startDateFilter);
                }

                if (endDate != null)
                {
                    DateTime endDateValue = DateTime.Parse(endDate);
                    Expression<Func<RoomDay, bool>> endDateFilter = roomDay => roomDay.Date.Date <= endDateValue.Date;
                    expressions.Add(endDateFilter);
                }

                if (available != null)
                {
                    Expression<Func<RoomDay, bool>> availableFilter = roomDay => roomDay.Available == available;
                    expressions.Add(availableFilter);
                }

                customExpression.Filter = ExpressionCombiner.CombineExpressions(expressions);
                var roomDays = await _roomDayRepository.GetFilteredItemsAsync(customExpression);
                var roomDaysToReturn = _mapper.Map<List<RoomDayDto>>(roomDays);
                return roomDaysToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the GetRoomDaysForRoom request.");
                throw;
            }
        }

        public async Task ExtendRoomDaysPeriodAsync(Guid roomId, int numOfDays, decimal price)
        {
            try
            {
                var room = await _roomRepository.GetByIdAsync(roomId);
                if (room == null)
                {
                    throw new ElementNotFoundException();
                }
                var roomDay = await _roomDayRepository.GetLastRoomDay(room.Id);
                var roomDays = new List<RoomDay>();
                var currentDate = DateTime.Now;
                if(roomDay != null)
                {
                    currentDate = roomDay.Date;
                }
                currentDate.AddDays(1);
                while (numOfDays > 0)
                {
                    numOfDays--;
                    var newRoomDay = new RoomDay
                    {
                        RoomId = roomId,
                        Available = true,
                        Price = price,
                        Date = currentDate,
                    };
                    roomDays.Add(newRoomDay);
                    currentDate.AddDays(1);
                }
                await _roomDayRepository.AddAllAsync(roomDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding room days");
                throw;
            }
        }

        private async Task<RoomDay> GetRoomDayAsync(Guid roomId, string dateString)
        {
            var date = DateTime.Parse(dateString);
            var roomDay = await _roomDayRepository.GetFirstOrDefaultAsync(rd => rd.RoomId == roomId && rd.Date == date);
            if (roomDay == null)
            {
                throw new ElementNotFoundException();
            }
            return roomDay;
        }

        private async Task ValidateRoomDayAsync(RoomDay entity)
        {
            var validationResults = await _roomDayValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
