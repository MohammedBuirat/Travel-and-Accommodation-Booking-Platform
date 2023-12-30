using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public Booking Booking { get; set; }
        public Guid BookingId { get; set; }
        public DateTimeOffset Date { get; set; }

        public override string ToString()
        {
            return $"{Id}, {UserId}, {Method}, {Amount}, {BookingId}, {Date}";
        }

        public Payment Clone()
        {
            return new Payment
            {
                Id = this.Id,
                User = null,
                UserId = this.UserId,
                Method = this.Method,
                Amount = this.Amount,
                Booking = null,
                BookingId = this.BookingId,
                Date = this.Date
            };
        }
    }
}
