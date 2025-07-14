using System.ComponentModel.DataAnnotations;

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
        public decimal Amount { get; set; }
        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft; // Default status
        public Job Job { get; set; }
        public Client Client { get; set; }
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