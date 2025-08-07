using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tradetrackr.api.Models
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ClientId { get; set; }
        public string UserId { get; set; } // From Auth0

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 0;

        public DateTime? PaymentDate { get; set; }

        public string? Notes { get; set; }

        public string? Terms { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft; // Default status

        // Navigation properties
        public Job Job { get; set; }
        public Client Client { get; set; }

        // Calculated properties
        [NotMapped]
        public decimal TotalAmount => Amount + TaxAmount;

        [NotMapped]
        public bool IsOverdue => Status != InvoiceStatus.Paid && DueDate < DateTime.UtcNow;

        [NotMapped]
        public bool IsPaid => Status == InvoiceStatus.Paid;

        [NotMapped]
        public int DaysUntilDue => (DueDate.Date - DateTime.UtcNow.Date).Days;
    }

    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue,
        Cancelled
    }
}