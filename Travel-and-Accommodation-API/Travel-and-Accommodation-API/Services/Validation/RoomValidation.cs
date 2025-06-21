using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class RoomValidation : AbstractValidator<Room>
    {
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly IRepository<Room> _roomRepository;

        public const int MaxDescriptionLength = 1000;

        public RoomValidation(IRepository<Hotel> hotelRepository,
            IRepository<Room> roomRepository)
        {
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));

            // Room Number
            RuleFor(room => room.RoomNumber)
                .NotNull().WithMessage("Room number is required")
                .MustAsync((room, roomNumber, cancellation) => RoomNumberUniqueByHotelAsync(roomNumber, room.Id, room.HotelId))
                .WithMessage("Room number must be unique within the hotel");

            // Description
            RuleFor(room => room.Description)
                .MaximumLength(MaxDescriptionLength).WithMessage($"Description must not exceed {MaxDescriptionLength} characters")
                .When(room => !string.IsNullOrEmpty(room.Description));

            // Adults Capacity
            RuleFor(room => room.AdultsCapacity)
                .NotNull().WithMessage("Adults capacity is required");

            // Children Capacity
            RuleFor(room => room.ChildrenCapacity)
                .NotNull().WithMessage("Children capacity is required");

            // Room Type
            RuleFor(room => room.Type)
                .NotNull().WithMessage("Room type is required")
                .IsInEnum().WithMessage("Invalid room type");

            // Amenities
            RuleFor(room => room.Amenities)
                .NotNull().WithMessage("Amenities are required")
                .IsInEnum().WithMessage("Invalid room amenities");

            // Hotel ID
            RuleFor(room => room.HotelId)
                .MustAsync(async (hotelId, cancellation) => await HotelExistsAsync(hotelId))
                .WithMessage(hotelId => $"Hotel with ID {hotelId} not found");
        }

        private async Task<bool> HotelExistsAsync(Guid hotelId)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            return hotel != null;
        }

        private async Task<bool> RoomNumberUniqueByHotelAsync(int roomNumber, Guid roomId, Guid hotelId)
        {
            var room = await _roomRepository.GetFirstOrDefaultAsync(r => r.HotelId == hotelId && r.RoomNumber == roomNumber);
            return room == null || (room != null && room.Id == roomId);
        }
    }
}
