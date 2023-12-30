using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.RoomDay;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IRoomDayService
    {
        public Task InsertRoomDaysAsync(Room room);
        public Task<RoomDayDto> GetAsync(Guid roomId, string date);
        public Task UpdateAsync(Guid roomId, string date, [FromBody] RoomDayDto roomDayToUpdate);
        public Task PartialUpdateAsync(Guid roomId, string date, JsonPatchDocument<RoomDay> jsonPatchDocument);
        public Task<IEnumerable<RoomDayDto>> GetRoomDaysForRoomAsync(Guid roomId, string? beginDate, string? endDate, bool? available);
        public Task ExtendRoomDaysPeriodAsync(Guid roomId, int numOfDays, decimal price);

    }
}
