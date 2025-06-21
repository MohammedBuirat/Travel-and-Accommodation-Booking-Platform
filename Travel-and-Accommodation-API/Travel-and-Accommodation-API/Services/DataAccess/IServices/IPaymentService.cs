using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.Payment;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IPaymentService
    {
        public Task<IEnumerable<PaymentDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<PaymentDto> GetByIdAsync(Guid id, ClaimsPrincipal user);
        public Task DeleteAsync(Guid id, ClaimsPrincipal user);
        public Task UpdateAsync(Guid id, PaymentDto updatedPayment, ClaimsPrincipal user);
        public Task<Payment> AddAsync(PaymentToAdd newPayment);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<Payment> patchDocument, ClaimsPrincipal user);
        public Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId, int? pageSize, int? pageNumber, ClaimsPrincipal user);
    }
}
