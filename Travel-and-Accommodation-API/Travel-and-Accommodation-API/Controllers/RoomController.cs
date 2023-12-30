using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Room;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using static Travel_and_Accommodation_API.Models.Enums;
using Travel_and_Accommodation_API.Views;
using Microsoft.AspNetCore.Authorization;
using Travel_and_Accommodation_API.Exceptions_and_logs;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomController> _logger;


        public RoomController(IRoomService roomService,ILogger<RoomController> logger)
        {
            _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("rooms")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms(int? pageSize, int? pageNumber)
        {
            try
            {
                var rooms = await _roomService.GetAllAsync(pageSize, pageNumber);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                Logs<Room>.GetEntitiesException(_logger, ex, "Room");
                throw;
            }
        }

        [HttpGet("rooms/{id}")]
        public async Task<ActionResult<RoomDto>> GetRoomById(Guid id)
        {
            try
            {
                var room = await _roomService.GetByIdAsync(id);
                return Ok(room);
            }
            catch (Exception ex)
            {
                Logs<Room>.GetEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        [HttpDelete("rooms/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteRoom(Guid id)
        {
            try
            {
                await _roomService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Room>.DeleteEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        [HttpPut("rooms/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRoom(Guid id, [FromBody] RoomToAdd roomToUpdate)
        {
            try
            {
                if (roomToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _roomService.UpdateAsync(id, roomToUpdate);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Room>.UpdateEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        [HttpPost("rooms")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddRoom([FromBody] RoomToAdd room)
        {
            try
            {
                if (room == null)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var newRoom = await _roomService.AddAsync(room);
                return CreatedAtAction(nameof(GetRooms), new { id = newRoom.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<Room>.AddEntityException(_logger, ex, "Room");
                throw;
            }
        }

        [HttpPatch("rooms/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PartialUpdateRoom(Guid id, JsonPatchDocument<Room> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    return BadRequest();
                }

                await _roomService.PartialUpdateAsync(id, jsonPatchDocument);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Room>.UpdateEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        [HttpGet("hotels/{hotelId}/rooms")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetHotelRooms(Guid hotelId, int? pageSize, int? pageNumber)
        {
            try
            {
                var rooms = await _roomService.GetHotelRoomsAsync(hotelId, pageSize, pageNumber);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the GetHotelRooms request.");
                throw;
            }
        }

        [HttpPost("rooms-filter/{hotelId}")]
        public async Task<ActionResult<IEnumerable<RoomWithPriceDto>>> GetSearchedFilteredRooms(Guid hotelId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, [FromBody] List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, RoomType? roomType, bool? descendingOrder)
        {
            try
            {
                if ((numberOfAdults + numberOfChildren) == 0)
                    return BadRequest("Number of adults and children are required.");
                if (checkInDate >= checkOutDate)
                {
                    return BadRequest("Check in date should be less than check out date");
                }

                var rooms = await _roomService.GetSearchedFilteredRoomsAsync(hotelId, numberOfAdults, numberOfChildren,
                    checkInDate, checkOutDate, pageSize, pageNumber, amenities, sort, maxPrice, minPrice, roomType, descendingOrder);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetSearchedFilteredRooms: {ex.Message}");
                throw;
            }
        }
    }
}
