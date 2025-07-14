using tradetrackr.api.Models;

namespace tradetrackr.api.Dto
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ClientId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}