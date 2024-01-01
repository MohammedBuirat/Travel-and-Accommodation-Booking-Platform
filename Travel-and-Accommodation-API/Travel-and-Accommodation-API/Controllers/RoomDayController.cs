using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.RoomDay;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class RoomDayController : ControllerBase
    {
        private readonly IRoomDayService _roomDayService;
        private readonly ILogger<RoomDayController> _logger;

        public RoomDayController(
            IRoomDayService roomDayService,
            ILogger<RoomDayController> logger)
        {
            _roomDayService = roomDayService ?? throw new ArgumentNullException(nameof(roomDayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet("roomdays/{roomId}/{date}")]
        public async Task<ActionResult<RoomDayDto>> GetRoomDay(Guid roomId, string date)
        {
            try
            {
                var roomDay = await _roomDayService.GetAsync(roomId, date);
                return Ok(roomDay);
            }
            catch (Exception ex)
            {
                Logs<RoomDay>.GetEntityException(_logger, ex, "RoomDay", roomId);
                throw;
            }
        }

        [HttpPut("roomdays/{roomId}/{date}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRoomDay(Guid roomId, string date, [FromBody] RoomDayDto roomDayToUpdate)
        {
            try
            {
                if (roomDayToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _roomDayService.UpdateAsync(roomId, date, roomDayToUpdate);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<RoomDay>.UpdateEntityException(_logger, ex, "RoomDay", roomId);
                throw;
            }
        }

        [HttpPatch("roomdays/{roomId}/{date}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PartialUpdateRoomDay(Guid roomId, string date, JsonPatchDocument<RoomDay> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    return BadRequest();
                }

                await _roomDayService.PartialUpdateAsync(roomId, date, jsonPatchDocument);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<RoomDay>.UpdateEntityException(_logger, ex, "RoomDay", roomId);
                throw;
            }
        }

        [HttpGet("rooms/{roomId}/roomdays")]
        public async Task<ActionResult<RoomDayDto>> GetRoomDaysForRoom(Guid roomId, string? beginDate, string? endDate, bool? available)
        {
            try
            {
                var roomDays = await _roomDayService.GetRoomDaysForRoomAsync(roomId, beginDate, endDate, available);
                return Ok(roomDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the GetRoomDaysForRoom request.");
                throw;
            }
        }

        [HttpGet("rooms/{roomId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ExtendRoomDaysPeriod(Guid roomId, int numOfDays, decimal price)
        {
            try
            {
                await _roomDayService.ExtendRoomDaysPeriodAsync(roomId, numOfDays, price);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding room days");
                throw;
            }
        }
    }
}
