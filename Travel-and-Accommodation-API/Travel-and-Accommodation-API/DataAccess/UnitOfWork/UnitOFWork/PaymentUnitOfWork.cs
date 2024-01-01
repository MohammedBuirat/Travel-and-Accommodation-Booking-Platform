using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public class PaymentUnitOfWork : IPaymentUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<PaymentUnitOfWork> _logger;
        IRepository<Payment> PaymentRepository { get; }
        IRepository<Booking> BookingRepository { get; }


        public PaymentUnitOfWork(TravelAndAccommodationContext dbContext,
            IRepository<Payment> paymentRepository,
            IRepository<Booking> bookingRepository,
            ILogger<PaymentUnitOfWork> logger)
        {
            _dbContext = dbContext ??
                throw new ArgumentNullException(nameof(dbContext));
            PaymentRepository = paymentRepository ??
                throw new ArgumentNullException(nameof(logger));
            BookingRepository = bookingRepository;
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Payment> InsertPaymentAsync(Payment payment)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    Guid bookingId = payment.BookingId;
                    var bookingToUpdate = await BookingRepository.GetByIdAsync(bookingId);

                    bookingToUpdate.Paid = true;
                    await BookingRepository.UpdateAsync(bookingToUpdate);
                    var paymentToReturn = await PaymentRepository.AddAsync(payment);
                    transaction.Commit();
                    return payment;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error while inserting payment");
                    throw;
                }
            }
        }

    }
}
