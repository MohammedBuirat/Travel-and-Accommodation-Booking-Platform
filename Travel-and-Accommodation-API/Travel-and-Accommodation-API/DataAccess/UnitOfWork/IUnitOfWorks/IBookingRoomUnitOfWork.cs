using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public interface IBookingRoomUnitOfWork
    {
        public Task<BookingRoom?> InsertBookingRoomAsync(BookingRoom bookingRoomToAdd);
        public Task DeleteBookingRoomAsync(Guid id);
    }
}