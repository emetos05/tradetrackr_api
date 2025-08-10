using Microsoft.EntityFrameworkCore;
using tradetrackr.api.Data;
using tradetrackr.api.Models;

namespace tradetrackr.api.Services
{
    /// <summary>
    /// Background service that automatically updates invoice statuses to Overdue when due dates have passed.
    /// </summary>
    public class InvoiceStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InvoiceStatusUpdateService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Check every 12 hours

        public InvoiceStatusUpdateService(
            IServiceProvider serviceProvider,
            ILogger<InvoiceStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateOverdueInvoicesAsync();
                    _logger.LogInformation("Invoice status update check completed at {Time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating invoice statuses");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task UpdateOverdueInvoicesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TradeTrackrDbContext>();

            var currentDate = DateTime.UtcNow.Date;

            // Find invoices that are overdue but not marked as such
            var overdueInvoices = await context.Invoices
                .Where(i => i.Status != InvoiceStatus.Paid && 
                           i.Status != InvoiceStatus.Cancelled && 
                           i.Status != InvoiceStatus.Overdue &&
                           i.DueDate.Date < currentDate)
                .ToListAsync();

            if (overdueInvoices.Any())
            {
                _logger.LogInformation("Found {Count} invoices to mark as overdue", overdueInvoices.Count);

                foreach (var invoice in overdueInvoices)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                    _logger.LogDebug("Updated invoice {InvoiceId} status to Overdue (due date: {DueDate})", 
                        invoice.Id, invoice.DueDate);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated {Count} invoices to Overdue status", overdueInvoices.Count);
            }
        }
    }
}