using System.ComponentModel.DataAnnotations;

namespace tradetrackr.api.Models
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid JobId { get; set; }
        [Required]
        public Guid ClientId { get; set; }
        [Required]
        public string UserId { get; set; } // From Auth0
        [Required]
        public DateTime IssueDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Status { get; set; } // e.g., "Draft", "Sent", "Paid"
        public Job Job { get; set; }
        public Client Client { get; set; }
    }
}
