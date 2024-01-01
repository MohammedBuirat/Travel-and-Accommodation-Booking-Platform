using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public interface IPaymentUnitOfWork
    {
        public Task<Payment> InsertPaymentAsync(Payment payment);
    }
}
