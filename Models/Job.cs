namespace tradetrackr.api.Models
{
    public class Job
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
