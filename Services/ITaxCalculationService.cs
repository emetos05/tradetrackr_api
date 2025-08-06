namespace tradetrackr.api.Services
{
    public interface ITaxCalculationService
    {
        decimal CalculateTaxAmount(decimal amount, decimal taxRate);
        decimal CalculateTotalAmount(decimal amount, decimal taxAmount);
        decimal CalculateTaxRate(decimal amount, decimal taxAmount);
        bool IsValidTaxRate(decimal taxRate);
    }
}
