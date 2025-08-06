using Mapster;
using Microsoft.EntityFrameworkCore;
using tradetrackr.api.Data;
using tradetrackr.api.Dto;
using tradetrackr.api.Models;

namespace tradetrackr.api.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly TradeTrackrDbContext _context;
        private readonly ITaxCalculationService _taxCalculationService;

        public InvoiceService(TradeTrackrDbContext context, ITaxCalculationService taxCalculationService)
        {
            _context = context;
            _taxCalculationService = taxCalculationService;
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, string userId)
        {
            // Validate due date
            if (!await ValidateDueDateAsync(request.DueDate))
            {
                throw new ArgumentException("Due date cannot be in the past");
            }

            // Verify job and client exist and belong to user
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == request.JobId && j.UserId == userId);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.ClientId && c.UserId == userId);

            if (job == null)
            {
                throw new ArgumentException("Job not found or does not belong to the current user");
            }

            if (client == null)
            {
                throw new ArgumentException("Client not found or does not belong to the current user");
            }

            // Calculate tax amount
            var taxAmount = _taxCalculationService.CalculateTaxAmount(request.Amount, request.TaxRate);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                JobId = request.JobId,
                ClientId = request.ClientId,
                UserId = userId,
                IssueDate = request.IssueDate,
                DueDate = request.DueDate,
                Amount = request.Amount,
                TaxRate = request.TaxRate,
                TaxAmount = taxAmount,
                Notes = request.Notes,
                Terms = request.Terms,
                Status = InvoiceStatus.Draft
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return MapToDto(invoice);
        }

        public async Task<InvoiceDto> UpdateInvoiceAsync(Guid id, UpdateInvoiceRequest request, string userId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found or does not belong to the current user");
            }

            // Validate due date
            if (!await ValidateDueDateAsync(request.DueDate))
            {
                throw new ArgumentException("Due date cannot be in the past");
            }

            // Verify job and client exist and belong to user
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == request.JobId && j.UserId == userId);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.ClientId && c.UserId == userId);

            if (job == null)
            {
                throw new ArgumentException("Job not found or does not belong to the current user");
            }

            if (client == null)
            {
                throw new ArgumentException("Client not found or does not belong to the current user");
            }

            // Calculate tax amount
            var taxAmount = _taxCalculationService.CalculateTaxAmount(request.Amount, request.TaxRate);

            // Update invoice properties
            invoice.JobId = request.JobId;
            invoice.ClientId = request.ClientId;
            invoice.IssueDate = request.IssueDate;
            invoice.DueDate = request.DueDate;
            invoice.Amount = request.Amount;
            invoice.TaxRate = request.TaxRate;
            invoice.TaxAmount = taxAmount;
            invoice.Notes = request.Notes;
            invoice.Terms = request.Terms;
            invoice.Status = request.Status;

            await _context.SaveChangesAsync();

            return MapToDto(invoice);
        }

        public async Task<InvoiceDto> UpdateInvoiceStatusAsync(Guid id, UpdateInvoiceStatusRequest request, string userId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found or does not belong to the current user");
            }

            invoice.Status = request.Status;

            // Clear payment date if status is not Paid
            if (request.Status != InvoiceStatus.Paid)
            {
                invoice.PaymentDate = null;
            }

            await _context.SaveChangesAsync();

            return MapToDto(invoice);
        }

        public async Task<InvoiceDto> RecordPaymentAsync(Guid id, RecordPaymentRequest request, string userId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found or does not belong to the current user");
            }

            // Validate payment amount matches invoice total
            var totalAmount = _taxCalculationService.CalculateTotalAmount(invoice.Amount, invoice.TaxAmount);
            if (Math.Abs(request.Amount - totalAmount) > 0.01m) // Allow for small rounding differences
            {
                throw new ArgumentException($"Payment amount ({request.Amount:C}) does not match invoice total ({totalAmount:C})");
            }

            invoice.PaymentDate = request.PaymentDate;
            invoice.Status = InvoiceStatus.Paid;

            await _context.SaveChangesAsync();

            return MapToDto(invoice);
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id, string userId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            return invoice == null ? null : MapToDto(invoice);
        }

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(string userId)
        {
            var invoices = await _context.Invoices
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return invoices.Select(MapToDto);
        }

        public async Task<bool> DeleteInvoiceAsync(Guid id, string userId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (invoice == null)
            {
                return false;
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GenerateInvoiceNumberAsync(string userId)
        {
            // Get the latest invoice number for this user
            var latestInvoice = await _context.Invoices
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.IssueDate)
                .FirstOrDefaultAsync();

            // Generate invoice number based on year and sequence
            var year = DateTime.UtcNow.Year;
            var sequence = 1;

            if (latestInvoice != null && latestInvoice.IssueDate.Year == year)
            {
                // Extract sequence number from existing invoices this year
                var thisYearInvoicesCount = await _context.Invoices
                    .CountAsync(i => i.UserId == userId && i.IssueDate.Year == year);
                sequence = thisYearInvoicesCount + 1;
            }

            return $"INV-{year}-{sequence:D4}";
        }

        public async Task<bool> ValidateDueDateAsync(DateTime dueDate)
        {
            // Due date should not be in the past (allow same day)
            return dueDate.Date >= DateTime.UtcNow.Date;
        }

        private InvoiceDto MapToDto(Invoice invoice)
        {
            var dto = invoice.Adapt<InvoiceDto>();

            // Set calculated properties
            dto.TotalAmount = invoice.TotalAmount;
            dto.IsOverdue = invoice.IsOverdue;
            dto.IsPaid = invoice.IsPaid;
            dto.DaysUntilDue = invoice.DaysUntilDue;

            return dto;
        }
    }
}
