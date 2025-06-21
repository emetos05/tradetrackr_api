namespace tradetrackr.api.Dto
{
    public class JobDto
    {
        public Guid ClientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal HourlyRate { get; set; }
        public float HoursWorked { get; set; }
        public decimal MaterialCost { get; set; }
    }
}