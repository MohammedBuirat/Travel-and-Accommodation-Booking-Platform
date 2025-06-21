using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public class BookingUnitOfWork : IBookingUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<BookingUnitOfWork> _logger;
        private readonly IRepository<Booking> BookingRepository;

        public BookingUnitOfWork(TravelAndAccommodationContext dbContext, ILogger<BookingUnitOfWork> logger,
            IRepository<Booking> bookingRepository)
        {
            _dbContext = dbContext ??
                throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            BookingRepository = bookingRepository ??
                throw new ArgumentNullException(nameof(bookingRepository));
        }

        public async Task<Booking> AddBookingAsync(Booking bookingToAdd)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    bool roomsAreAvalible = await RoomsAreAvailable(bookingToAdd);
                    if (!roomsAreAvalible)
                    {
                        throw new Exception();
                    }
                    await BookAllRooms(bookingToAdd);
                    var booking = await BookingRepository.AddAsync(bookingToAdd);
                    await InsertBookingRooms(booking.Id, bookingToAdd.BookingRooms);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return booking;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error while inserting booking");
                    throw;
                }
            }
        }

        private async Task<bool> RoomsAreAvailable(Booking bookingToAdd)
        {
            try
            {
                var tasks = bookingToAdd.BookingRooms.Select(async room =>
                {
                    Expression<Func<RoomDay, bool>> filter = rd =>
                        rd.RoomId == room.RoomId &&
                        rd.Available == false &&
                        rd.Date >= bookingToAdd.CheckInDate &&
                        rd.Date < bookingToAdd.CheckOutDate;

                    var bookedRoomDays = await _dbContext.Set<RoomDay>().FirstOrDefaultAsync(filter);
                    return bookedRoomDays == null;
                });

                var results = await Task.WhenAll(tasks);

                return results.All(result => result);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task BookAllRooms(Booking booking)
        {
            var roomIds = booking.BookingRooms.Select(room => room.RoomId).ToList();

            Expression<Func<RoomDay, bool>> filter = rd =>
                roomIds.Contains(rd.RoomId) &&
                rd.Available == false &&
                rd.Date >= booking.CheckInDate &&
                rd.Date < booking.CheckOutDate;

            var roomsToUpdate = await _dbContext.Set<RoomDay>().Where(filter).ToListAsync();

            foreach (var roomDay in roomsToUpdate)
            {
                roomDay.Available = true;
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task InsertBookingRooms(Guid id, List<BookingRoom> rooms)
        {
            var bookingRooms = rooms.Select(room => new BookingRoom
            {
                BookingId = id,
                RoomId = room.RoomId
            }).ToList();

            _dbContext.Set<BookingRoom>().AddRange(bookingRooms);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteBookingAsync(Guid id)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var booking = await BookingRepository.GetByIdAsync(id);
                    if (booking == null)
                    {
                        throw new Exception();
                    }
                    await DeleteBookingRooms(id);
                    await FreeAllRooms(booking);
                    await BookingRepository.DeleteAsync(booking.Id);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error while deleting booking");
                    throw;
                }
            }
        }

        private async Task DeleteBookingRooms(Guid id)
        {
            var bookingRoomsToDelete = await _dbContext.Set<BookingRoom>()
                .Where(br => br.BookingId == id)
                .ToListAsync();

            _dbContext.Set<BookingRoom>().RemoveRange(bookingRoomsToDelete);
            await _dbContext.SaveChangesAsync();
        }

        private async Task FreeAllRooms(Booking booking)
        {
            var roomIds = booking.BookingRooms.Select(room => room.RoomId).ToList();

            Expression<Func<RoomDay, bool>> filter = rd =>
                roomIds.Contains(rd.RoomId) &&
                rd.Available == false &&
                rd.Date >= booking.CheckInDate &&
                rd.Date < booking.CheckOutDate;

            await _dbContext.RoomDays
                .Where(filter)
                .ForEachAsync(entity =>
                {
                    entity.Available = true;
                    _dbContext.Entry(entity).State = EntityState.Modified;
                });

            await _dbContext.SaveChangesAsync();
        }
    }
}
