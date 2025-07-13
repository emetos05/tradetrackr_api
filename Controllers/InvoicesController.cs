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

namespace tradetrackr.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly TradeTrackrDbContext _context;

        public InvoicesController(TradeTrackrDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId() => HttpContext.User.FindFirst("sub")?.Value;

        /// <summary>
        /// Retrieves all invoices for the logged in user.
        /// </summary>
        /// <remarks>
        /// Returns a list of invoices including their associated client and job details.
        /// </remarks>
        /// <response code="200">Returns the list of invoices</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices()
        {
            var userId = GetCurrentUserId();
            var invoices = await _context.Invoices
                .Where(i => i.UserId == userId)
                .ProjectToType<InvoiceDto>()
                .ToListAsync();
            return invoices;
        }

        /// <summary>
        /// Retrieves a specific invoice by ID for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the invoice to retrieve</param>
        /// <remarks>
        /// Returns the invoice including its associated client and job details.
        /// </remarks>
        /// <response code="200">Returns the invoice</response>
        /// <response code="404">If the invoice is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
        {
            var userId = GetCurrentUserId();
            var invoice = await _context.Invoices
                .Where(i => i.Id == id && i.UserId == userId)
                .ProjectToType<InvoiceDto>()
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        /// <summary>
        /// Updates an existing invoice for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the invoice to update</param>
        /// <param name="invoiceDto">The updated invoice object</param>
        /// <remarks>
        /// Returns a 204 No Content response on success.
        /// Returns a 400 Bad Request if the ID does not match the invoice ID.
        /// Returns a 404 Not Found if the invoice does not exist.
        /// </remarks>
        /// <response code="204">Invoice updated successfully</response>
        /// <response code="400">ID does not match invoice ID</response>
        /// <response code="401">User is not authorized to update this invoice</response>
        /// <response code="404">Invoice not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] InvoiceDto invoiceDto)
        {
            var userId = GetCurrentUserId();
            var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (existingInvoice == null)
            {
                return NotFound();
            }
            existingInvoice.JobId = invoiceDto.JobId;
            existingInvoice.ClientId = invoiceDto.ClientId;
            existingInvoice.IssueDate = invoiceDto.IssueDate;
            existingInvoice.DueDate = invoiceDto.DueDate;
            existingInvoice.Amount = invoiceDto.Amount;
            existingInvoice.Status = invoiceDto.Status;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(existingInvoice.Adapt<InvoiceDto>());
        }

        /// <summary>
        /// Creates a new invoice for the logged in user.
        /// </summary>
        /// <param name="invoiceDto">The invoice object to create</param>
        /// <remarks>
        /// Returns the created invoice.
        /// Returns a 400 Bad Request if the model is invalid or if the job/client does not exist or does not belong to the user.
        /// </remarks>
        /// <response code="201">Invoice created successfully</response>
        /// <response code="400">Invalid model or job/client does not exist or belong to user</response>
        [HttpPost]
        public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] InvoiceDto invoiceDto)
        {
            var userId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == invoiceDto.JobId && j.UserId == userId);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == invoiceDto.ClientId && c.UserId == userId);
            if (job == null || client == null)
            {
                return BadRequest("Job or Client does not exist or does not belong to the current user.");
            }
            var invoice = new Invoice
            {
                JobId = invoiceDto.JobId,
                ClientId = invoiceDto.ClientId,
                IssueDate = invoiceDto.IssueDate,
                DueDate = invoiceDto.DueDate,
                Amount = invoiceDto.Amount,
                Status = invoiceDto.Status,
                UserId = userId
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoiceDto);
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
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (invoice == null)
            {
                return NotFound();
            }
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool InvoiceExists(Guid id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }
    }
}