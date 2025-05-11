namespace tradetrackr.api.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
        
}
