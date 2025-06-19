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
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly TradeTrackrDbContext _context;

        public InvoicesController(TradeTrackrDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId() => HttpContext.User.FindFirst("sub")?.Value;

        // GET: api/Invoices
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

        // GET: api/Invoices/5
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

        // PUT: api/Invoices/5
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

        // POST: api/Invoices
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

        // DELETE: api/Invoices/5
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
