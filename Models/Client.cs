namespace tradetrackr.api.Models
{
    public class Client
    {
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
