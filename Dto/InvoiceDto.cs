using tradetrackr.api.Models;
using System.ComponentModel.DataAnnotations;

namespace tradetrackr.api.Dto
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }

        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        public decimal TaxAmount { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal TaxRate { get; set; } = 0;

        public DateTime? PaymentDate { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [StringLength(500, ErrorMessage = "Terms cannot exceed 500 characters")]
        public string? Terms { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        // Read-only calculated properties
        public decimal TotalAmount { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsPaid { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class CreateInvoiceRequest
    {
        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal TaxRate { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [StringLength(500, ErrorMessage = "Terms cannot exceed 500 characters")]
        public string? Terms { get; set; }
    }

    public class UpdateInvoiceRequest
    {
        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal TaxRate { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [StringLength(500, ErrorMessage = "Terms cannot exceed 500 characters")]
        public string? Terms { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }
    }

    public class UpdateInvoiceStatusRequest
    {
        [Required]
        public InvoiceStatus Status { get; set; }
    }

    public class RecordPaymentRequest
    {
        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}