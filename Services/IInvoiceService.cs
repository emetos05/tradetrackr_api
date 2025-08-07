using tradetrackr.api.Dto;
using tradetrackr.api.Models;

namespace tradetrackr.api.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, string userId);
        Task<InvoiceDto> UpdateInvoiceAsync(Guid id, UpdateInvoiceRequest request, string userId);
        Task<InvoiceDto> UpdateInvoiceStatusAsync(Guid id, UpdateInvoiceStatusRequest request, string userId);
        Task<InvoiceDto> RecordPaymentAsync(Guid id, RecordPaymentRequest request, string userId);
        Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id, string userId);
        Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(string userId);
        Task<bool> DeleteInvoiceAsync(Guid id, string userId);
        Task<string> GenerateInvoiceNumberAsync(string userId);
        Task<bool> ValidateDueDateAsync(DateTime dueDate);
    }
}
