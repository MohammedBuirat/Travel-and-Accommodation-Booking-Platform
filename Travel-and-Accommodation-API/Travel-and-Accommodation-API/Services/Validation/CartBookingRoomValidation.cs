using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class CartBookingRoomValidation : AbstractValidator<CartBookingRoom>
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<CartBooking> _cartBookingRepository;

        public CartBookingRoomValidation(IRepository<Room> roomRepository,
            IRepository<CartBooking> cartBookingRepository)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
            _cartBookingRepository = cartBookingRepository ?? throw new ArgumentNullException(nameof(cartBookingRepository));

            // Room ID
            RuleFor(cartBookingRoom => cartBookingRoom.RoomId)
                .NotEmpty().WithMessage("Room ID is required")
                .MustAsync(async (roomId, cancellation) => await RoomExistsAsync(roomId))
                .WithMessage(roomId => $"Room with ID {roomId} not found");

            // Cart Booking ID
            RuleFor(cartBookingRoom => cartBookingRoom.CartBookingId)
                .NotEmpty().WithMessage("Cart Booking ID is required")
                .MustAsync(async (cartBookingId, cancellation) => await CartBookingExistsAsync(cartBookingId))
                .WithMessage(cartBookingId => $"Cart Booking with ID {cartBookingId} not found");
        }

        private async Task<bool> RoomExistsAsync(Guid roomId)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);
            return room != null;
        }

        private async Task<bool> CartBookingExistsAsync(Guid cartBookingId)
        {
            var cartBooking = await _cartBookingRepository.GetByIdAsync(cartBookingId);
            return cartBooking != null;
        }
    }
}
