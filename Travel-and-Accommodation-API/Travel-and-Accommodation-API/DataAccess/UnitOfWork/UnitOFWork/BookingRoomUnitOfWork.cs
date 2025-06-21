using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public class BookingRoomUnitOfWork : IBookingRoomUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<BookingRoomUnitOfWork> _logger;

        public BookingRoomUnitOfWork(TravelAndAccommodationContext dbContext, ILogger<BookingRoomUnitOfWork> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BookingRoom?> InsertBookingRoomAsync(BookingRoom bookingRoomToAdd)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var booking = await _dbContext.Set<Booking>().FindAsync(bookingRoomToAdd.BookingId);
                    if (booking == null)
                    {
                        throw new InvalidOperationException();
                    }

                    bool roomIsAvalible = await RoomIsAvalible(booking, bookingRoomToAdd);
                    if (!roomIsAvalible)
                    {
                        throw new Exception();
                    }
                    var bookingRoom = await _dbContext.Set<BookingRoom>().AddAsync(bookingRoomToAdd);

                    var totalNewRoomPrice = await GetTotalRoomPrice(booking, bookingRoomToAdd);

                    await ChangeRoomDayAvailability(booking, bookingRoomToAdd, false);

                    booking.TotalPrice += totalNewRoomPrice;

                    await _dbContext.SaveChangesAsync();

                    var insertedBookingRoom = bookingRoom.Entity;

                    await transaction.CommitAsync();

                    return insertedBookingRoom;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error occurred while inserting booking room.");
                    throw;
                }
            }
        }

        private async Task<bool> RoomIsAvalible(Booking booking, BookingRoom bookingRoomToAdd)
        {
            Expression<Func<RoomDay, bool>> searchFilter = rd => rd.RoomId == booking.Id &&
                    rd.Date >= booking.CheckInDate && rd.Date < booking.CheckOutDate
                    && rd.Available == false;

            var roomBookedDays = await _dbContext.Set<RoomDay>().FirstOrDefaultAsync(searchFilter);
            if (roomBookedDays != null)
            {
                return false;
            }
            return true;
        }

        private async Task<decimal> GetTotalRoomPrice(Booking booking, BookingRoom bookingRoom)
        {
            Expression<Func<RoomDay, bool>> filter = rd =>
                rd.RoomId == bookingRoom.RoomId &&
                rd.Date >= booking.CheckInDate &&
                rd.Date < booking.CheckOutDate;

            var totalRoomPrice = await _dbContext.Set<RoomDay>()
                .Where(filter)
                .GroupBy(rd => rd.RoomId)
                .Select(group => new
                {
                    RoomId = group.Key,
                    TotalPrice = group.Sum(rd => rd.Price)
                })
                .FirstOrDefaultAsync();

            return totalRoomPrice?.TotalPrice ?? 0;
        }

        public async Task DeleteBookingRoomAsync(Guid id)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var bookingRoomToDelete = await _dbContext.Set<BookingRoom>().FindAsync(id);
                    if (bookingRoomToDelete == null)
                    {
                        throw new Exception();
                    }
                    var booking = await _dbContext.Set<Booking>().FindAsync(bookingRoomToDelete.BookingId);
                    if (booking == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var bookingRoomEntry = _dbContext.Set<BookingRoom>().Remove(bookingRoomToDelete);

                    var totalRoomPrice = await GetTotalRoomPrice(booking, bookingRoomToDelete);

                    booking.TotalPrice -= totalRoomPrice;

                    await ChangeRoomDayAvailability(booking, bookingRoomToDelete, true);

                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error occurred while deleting booking room.");
                    throw;
                }
            }
        }

        private async Task ChangeRoomDayAvailability(Booking booking, BookingRoom bookingRoom, bool available)
        {
            Expression<Func<RoomDay, bool>> filter = rd =>
                rd.RoomId == bookingRoom.RoomId &&
                rd.Date >= booking.CheckInDate &&
                rd.Date < booking.CheckOutDate;

            await _dbContext.Set<RoomDay>()
                .Where(filter)
                .ForEachAsync(roomDay =>
                {
                    roomDay.Available = available;
                    _dbContext.Entry(roomDay).State = EntityState.Modified;
                });

            await _dbContext.SaveChangesAsync();
        }
    }
}
