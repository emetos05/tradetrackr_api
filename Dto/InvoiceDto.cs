namespace tradetrackr.api.Dto
{
    public class InvoiceDto
    {
        public Guid JobId { get; set; }
        public Guid ClientId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}