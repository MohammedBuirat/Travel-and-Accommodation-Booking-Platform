using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class BookingRoomValidation : AbstractValidator<BookingRoom>
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<Room> _roomReopsitory;

        public BookingRoomValidation(IRepository<Booking> bookingRepository
            , IRepository<Room> roomReopsitory)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _roomReopsitory = roomReopsitory ?? throw new ArgumentNullException(nameof(roomReopsitory));

            // Booking ID
            RuleFor(bookingRoom => bookingRoom.BookingId)
                .NotEmpty().WithMessage("Booking ID is required")
                .MustAsync(async (bookingId, cancellation) => await BookingExistsAsync(bookingId))
                .WithMessage(bookingId => $"Booking with ID {bookingId} not found");

            // Room ID
            RuleFor(bookingRoom => bookingRoom.RoomId)
                .NotEmpty().WithMessage("Room ID is required")
                .MustAsync(async (roomId, cancellation) => await RoomExistsAsync(roomId))
                .WithMessage(roomId => $"Room with ID {roomId} not found");
        }

        private async Task<bool> BookingExistsAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            return booking != null;
        }

        private async Task<bool> RoomExistsAsync(Guid roomId)
        {
            var room = await _roomReopsitory.GetByIdAsync(roomId);
            return room != null;
        }
    }
}
