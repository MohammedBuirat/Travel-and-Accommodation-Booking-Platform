using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Payment;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("payments")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments(int? pageSize, int? pageNumber)
        {
            try
            {
                var payments = await _paymentService.GetAllAsync(pageSize, pageNumber);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                Logs<Payment>.GetEntitiesException(_logger, ex, "Payments");
                throw;
            }
        }

        [HttpGet("payments/{id}")]
        public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(id, User);
                if (payment == null)
                {
                    return NotFound();
                }
                return Ok(payment);
            }
            catch (Exception ex)
            {
                Logs<Payment>.GetEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        [HttpDelete("payments/{id}")]
        public async Task<ActionResult> DeletePayment(Guid id)
        {
            try
            {
                await _paymentService.DeleteAsync(id, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Payment>.DeleteEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        [HttpPut("payments/{id}")]
        public async Task<ActionResult> UpdatePayment(Guid id, [FromBody] PaymentDto updatedPayment)
        {
            try
            {
                if (updatedPayment == null)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _paymentService.UpdateAsync(id, updatedPayment, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Payment>.DeleteEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        [HttpPost("payments")]
        public async Task<ActionResult> AddPayment([FromBody] PaymentToAdd newPayment)
        {
            try
            {
                if (newPayment == null)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _paymentService.AddAsync(newPayment);
                return Created("Payment was created successfully", null);
            }
            catch (Exception ex)
            {
                Logs<PaymentToAdd>.AddEntityException(_logger, ex, "Payment");
                throw;
            }
        }

        [HttpPatch("payments/{id}")]
        public async Task<ActionResult> PartialUpdatePayment(Guid id, JsonPatchDocument<Payment> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return BadRequest();
                }

                await _paymentService.PartialUpdateAsync(id, patchDocument, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Payment>.UpdateEntityException(_logger, ex, "Payment", id);
                throw;
            }
        }

        [HttpGet("users/{userId}/payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(string userId, int? pageSize, int? pageNumber)
        {
            try
            {
                var payments = await _paymentService.GetUserPaymentsAsync(userId, pageSize, pageNumber, User);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the request in GetUserPayments method.");
                throw;
            }
        }
    }
}
