using System.ComponentModel.DataAnnotations;

namespace tradetrackr.api.Models
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal HourlyRate { get; set; }
        [Required]
        public float HoursWorked { get; set; }
        public decimal MaterialCost { get; set; }
        public Client Client { get; set; }
    }
}
