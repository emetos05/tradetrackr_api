using System.ComponentModel.DataAnnotations;

namespace tradetrackr.api.Models
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ClientId { get; set; }
        [Required]
        public string UserId { get; set; } // From Auth0
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal HourlyRate { get; set; }
        [Required]
        public float HoursWorked { get; set; }
        public decimal MaterialCost { get; set; }
        public Client Client { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
    }
}
