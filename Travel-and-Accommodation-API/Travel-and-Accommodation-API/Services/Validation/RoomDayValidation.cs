using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class RoomDayValidation : AbstractValidator<RoomDay>
    {
        private readonly IRepository<Room> _roomRepository;

        public RoomDayValidation(IRepository<Room> roomRepository)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));

            // Room ID
            RuleFor(roomDay => roomDay.RoomId)
                .NotEmpty().WithMessage("Room ID is required")
                .MustAsync(async (roomId, cancellation) => await RoomExistsAsync(roomId))
                .WithMessage(roomId => $"Room with ID {roomId} not found");

            // Price
            RuleFor(roomDay => roomDay.Price)
                .NotNull().WithMessage("Price is required")
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
        }

        private async Task<bool> RoomExistsAsync(Guid roomId)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);
            return room != null;
        }
    }
}
