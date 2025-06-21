using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Dto.Payment;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly ILogger<PaymentService> _logger;
        private readonly IPaymentUnitOfWork _paymentUnitOfWork;
        private readonly PaymentValidation _paymentValidation;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public PaymentService(IRepository<Payment> repository,
            ILogger<PaymentService> logger,
            IPaymentUnitOfWork paymentUnitOfWork,
            PaymentValidation paymentValidation,
            IMapper mapper,
            UserManager<User> userManager)
        {
            _paymentRepository = repository ??
                throw new ArgumentNullException(nameof(repository));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _paymentUnitOfWork = paymentUnitOfWork ??
                throw new ArgumentNullException(nameof(paymentUnitOfWork));
            _paymentValidation = paymentValidation ??
                throw new ArgumentNullException(nameof(paymentValidation));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ??
                throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IEnumerable<PaymentDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var payments = await _paymentRepository.GetAllAsync(paging);
                var paymentsToReturn = _mapper.Map<List<PaymentDto>>(payments);
                Logs<Payment>.GetEntitiesLog(_logger, "Payments");
                return paymentsToReturn;
            }
            catch (Exception ex)
            {
                Logs<Payment>.GetEntitiesException(_logger, ex, "Payments");
                throw;
            }
        }

        public async Task<PaymentDto> GetByIdAsync(Guid id, ClaimsPrincipal user)
        {
            try
            {
                var payment = await GetPaymentAsync(id);
                AuthorizeUser(user, payment.UserId);
                var paymentToReturn = _mapper.Map<PaymentDto>(payment);
                Logs<Payment>.GetEntityLog(_logger, "Payment", id);
                return paymentToReturn;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<Payment>.GetEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, ClaimsPrincipal user)
        {
            try
            {
                var payment = await GetPaymentAsync(id);
                AuthorizeUser(user, payment.UserId);
                await _paymentRepository.DeleteAsync(id);
                Logs<Payment>.DeleteEntityLog(_logger, "Payment", payment);
            }
            catch (Exception ex)
            {
                Logs<Payment>.DeleteEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, PaymentDto updatedPayment, ClaimsPrincipal user)
        {
            try
            {
                var oldPayment = await GetPaymentAsync(id);
                var newPayment = oldPayment.Clone();
                AuthorizeUser(user, oldPayment.UserId);
                _mapper.Map(updatedPayment, newPayment);
                await ValidatePaymentAsync(newPayment);
                await _paymentRepository.UpdateAsync(newPayment);
                Logs<Payment>.UpdateEntityLog(_logger, "Payment", oldPayment);
            }
            catch (Exception ex)
            {
                Logs<Payment>.UpdateEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        public async Task<Payment> AddAsync(PaymentToAdd newPayment)
        {
            try
            {
                var paymentToAdd = _mapper.Map<Payment>(newPayment);
                await ValidatePaymentAsync(paymentToAdd);
                var addedPayment = await _paymentUnitOfWork.InsertPaymentAsync(paymentToAdd);
                Logs<Payment>.AddEntityLog(_logger, "Payment", paymentToAdd);
                return addedPayment;
            }
            catch (Exception ex)
            {
                Logs<Payment>.AddEntityException(_logger, ex, "Payment", _mapper.Map<Payment>(newPayment));
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Payment> patchDocument, ClaimsPrincipal user)
        {
            try
            {
                var oldPayment = await GetPaymentAsync(id);
                var newPayment = oldPayment.Clone();
                AuthorizeUser(user, oldPayment.UserId);
                patchDocument.ApplyTo(newPayment);
                await ValidatePaymentAsync(newPayment);
                await _paymentRepository.UpdateAsync(newPayment);
                Logs<Payment>.UpdateEntityLog(_logger, "Payment", oldPayment);
            }
            catch (Exception ex)
            {
                Logs<Payment>.UpdateEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId, int? pageSize, int? pageNumber, ClaimsPrincipal user)
        {
            try
            {
                bool userExists = (await _userManager.FindByIdAsync(userId) != null);
                if (!userExists)
                {
                    throw new ElementNotFoundException();
                }
                AuthorizeUser(user, userId);
                var page = new Paging(pageSize, pageNumber);
                Expression<Func<Payment, bool>> filter = p => p.UserId == userId;
                var customExpression = new CustomExpression<Payment> { Filter = filter, Paging = page };
                var payments = await _paymentRepository.GetFilteredItemsAsync(customExpression);
                var paymentsToReturn = _mapper.Map<List<PaymentDto>>(payments);
                return paymentsToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the request in GetUserPayments method.");
                throw;
            }
        }

        private async Task<Payment> GetPaymentAsync(Guid id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new ElementNotFoundException();
            }
            return payment;
        }

        private async Task ValidatePaymentAsync(Payment entity)
        {
            var validationResults = await _paymentValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }

        private void AuthorizeUser(ClaimsPrincipal userClaims, string id)
        {
            var userId = userClaims.Claims.FirstOrDefault(c => c.Type == "Sub")?.Value;
            var userRole = userClaims.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "User";
            if (userId != id && userRole != "Admin")
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
