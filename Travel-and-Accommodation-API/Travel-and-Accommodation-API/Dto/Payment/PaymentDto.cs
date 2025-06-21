using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Dto.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public Guid BookingId { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
