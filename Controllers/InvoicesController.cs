using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tradetrackr.api.Data;
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
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var userId = GetCurrentUserId();
            return await _context.Invoices
                .Where(i => i.UserId == userId)
                .Include(i => i.Client)
                .Include(i => i.Job)
                .ToListAsync();
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
        public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
        {
            var userId = GetCurrentUserId();
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Job)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (invoice == null)
            {
                return NotFound();
            }

            return invoice;
        }

        /// <summary>
        /// Updates an existing invoice for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the invoice to update</param>
        /// <param name="invoice">The updated invoice object</param>
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
        public async Task<IActionResult> UpdateInvoice(Guid id, Invoice invoice)
        {
            var userId = GetCurrentUserId();
            if (id != invoice.Id)
            {
                return BadRequest();
            }
            if (invoice.UserId != userId)
            {
                return Unauthorized();
            }
            var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (existingInvoice == null)
            {
                return NotFound();
            }
            // Update allowed fields
            existingInvoice.IssueDate = invoice.IssueDate;
            existingInvoice.DueDate = invoice.DueDate;
            existingInvoice.Amount = invoice.Amount;
            existingInvoice.Status = invoice.Status;
            existingInvoice.ClientId = invoice.ClientId;
            existingInvoice.JobId = invoice.JobId;
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
            return NoContent();
        }

        /// <summary>
        /// Creates a new invoice for the logged in user.
        /// </summary>
        /// <param name="invoice">The invoice object to create</param>
        /// <remarks>
        /// Returns the created invoice.
        /// Returns a 400 Bad Request if the model is invalid or if the job/client does not exist or does not belong to the user.
        /// </remarks>
        /// <response code="201">Invoice created successfully</response>
        /// <response code="400">Invalid model or job/client does not exist or belong to user</response>
        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
        {
            var userId = GetCurrentUserId();
            invoice.UserId = userId;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Optionally: check that Job and Client exist and belong to user
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == invoice.JobId && j.UserId == userId);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == invoice.ClientId && c.UserId == userId);
            if (job == null || client == null)
            {
                return BadRequest("Job or Client does not exist or does not belong to the current user.");
            }
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
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
