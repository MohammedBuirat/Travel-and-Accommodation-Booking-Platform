using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public interface IBookingUnitOfWork
    {
        public Task<Booking> AddBookingAsync(Booking bookingToAdd);
        public Task DeleteBookingAsync(Guid id);
    }
}
