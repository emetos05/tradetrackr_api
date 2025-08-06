using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tradetrackr.api.Data;
using tradetrackr.api.Dto;
using tradetrackr.api.Models;
using tradetrackr.api.Services;

namespace tradetrackr.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly TradeTrackrDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(TradeTrackrDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        private string GetCurrentUserId() => HttpContext.User.FindFirst("sub")?.Value;

        /// <summary>
        /// Retrieves all invoices for the logged in user.
        /// </summary>
        /// <remarks>
        /// Returns a list of invoices including their calculated properties like total amount and overdue status.
        /// </remarks>
        /// <response code="200">Returns the list of invoices</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var invoices = await _invoiceService.GetInvoicesAsync(userId);
            return Ok(invoices);
        }

        /// <summary>
        /// Retrieves a specific invoice by ID for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the invoice to retrieve</param>
        /// <remarks>
        /// Returns the invoice including its calculated properties and associated data.
        /// </remarks>
        /// <response code="200">Returns the invoice</response>
        /// <response code="404">If the invoice is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var invoice = await _invoiceService.GetInvoiceByIdAsync(id, userId);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        /// <summary>
        /// Creates a new invoice for the logged in user.
        /// </summary>
        /// <param name="request">The invoice creation request</param>
        /// <remarks>
        /// Creates a new invoice with automatic tax calculations and validation.
        /// Returns a 400 Bad Request if the model is invalid or if the job/client does not exist or does not belong to the user.
        /// </remarks>
        /// <response code="201">Invoice created successfully</response>
        /// <response code="400">Invalid model or job/client does not exist or belong to user</response>
        [HttpPost]
        public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var invoice = await _invoiceService.CreateInvoiceAsync(request, userId);
                return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing invoice for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the invoice to update</param>
        /// <param name="request">The updated invoice data</param>
        /// <remarks>
        /// Updates the invoice with automatic tax recalculation and validation.
        /// Returns a 400 Bad Request if the ID does not match the invoice ID.
        /// Returns a 404 Not Found if the invoice does not exist.
        /// </remarks>
        /// <response code="200">Invoice updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User is not authorized to update this invoice</response>
        /// <response code="404">Invoice not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var invoice = await _invoiceService.UpdateInvoiceAsync(id, request, userId);
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the status of an invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to update</param>
        /// <param name="request">The status update request</param>
        /// <remarks>
        /// Updates only the status of the invoice. Automatically clears payment date if status is not Paid.
        /// </remarks>
        /// <response code="200">Invoice status updated successfully</response>
        /// <response code="400">Invalid status value</response>
        /// <response code="404">Invoice not found</response>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<InvoiceDto>> UpdateInvoiceStatus(Guid id, [FromBody] UpdateInvoiceStatusRequest request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var invoice = await _invoiceService.UpdateInvoiceStatusAsync(id, request, userId);
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Records a payment for an invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to record payment for</param>
        /// <param name="request">The payment details</param>
        /// <remarks>
        /// Records payment and automatically updates the invoice status to Paid.
        /// Validates that the payment amount matches the invoice total.
        /// </remarks>
        /// <response code="200">Payment recorded successfully</response>
        /// <response code="400">Invalid payment amount or date</response>
        /// <response code="404">Invoice not found</response>
        [HttpPost("{id}/payment")]
        public async Task<ActionResult<InvoiceDto>> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var invoice = await _invoiceService.RecordPaymentAsync(id, request, userId);
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes an invoice for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the invoice to delete</param>
        /// <remarks>
        /// Returns a 204 No Content response on success.
        /// Returns a 404 Not Found if the invoice does not exist.
        /// </remarks>
        /// <response code="204">Invoice deleted successfully</response>
        /// <response code="404">Invoice not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _invoiceService.DeleteInvoiceAsync(id, userId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Generates a new invoice number for the user.
        /// </summary>
        /// <remarks>
        /// Returns a formatted invoice number based on the current year and sequence.
        /// </remarks>
        /// <response code="200">Returns the generated invoice number</response>
        [HttpGet("generate-number")]
        public async Task<ActionResult<string>> GenerateInvoiceNumber()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var invoiceNumber = await _invoiceService.GenerateInvoiceNumberAsync(userId);
            return Ok(new { InvoiceNumber = invoiceNumber });
        }
    }
}