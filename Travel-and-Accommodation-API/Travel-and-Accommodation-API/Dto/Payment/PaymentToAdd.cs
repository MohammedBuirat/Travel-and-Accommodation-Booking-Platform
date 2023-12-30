using System.ComponentModel.DataAnnotations;
using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Dto.Payment
{
    public class PaymentToAdd
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public PaymentMethod Method { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public Guid BookingId { get; set; }
    }
}
