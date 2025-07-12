using tradetrackr.api.Models;

namespace tradetrackr.api.Dto
{
    public class JobDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public DateTime? CompletedAt { get; set; } 
        public decimal HourlyRate { get; set; }
        public float HoursWorked { get; set; }
        public decimal MaterialCost { get; set; }
    }
}