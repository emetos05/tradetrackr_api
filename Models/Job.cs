using System.ComponentModel.DataAnnotations;

namespace tradetrackr.api.Models
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string UserId { get; set; } // From Auth0
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public JobStatus Status { get; set; } = JobStatus.NotStarted;  // Default status
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; } // Nullable for ongoing jobs
        [Required]
        public decimal HourlyRate { get; set; }
        [Required]
        public float HoursWorked { get; set; }
        public decimal MaterialCost { get; set; }
        public Client Client { get; set; }
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    public enum JobStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Cancelled,
        OnHold
    }
}