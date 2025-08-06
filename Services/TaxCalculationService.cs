namespace tradetrackr.api.Services
{
    public class TaxCalculationService : ITaxCalculationService
    {
        public decimal CalculateTaxAmount(decimal amount, decimal taxRate)
        {
            if (amount <= 0 || taxRate < 0 || taxRate > 100)
            {
                return 0;
            }

            return Math.Round(amount * (taxRate / 100), 2);
        }

        public decimal CalculateTotalAmount(decimal amount, decimal taxAmount)
        {
            return Math.Round(amount + taxAmount, 2);
        }

        public decimal CalculateTaxRate(decimal amount, decimal taxAmount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            return Math.Round((taxAmount / amount) * 100, 2);
        }

        public bool IsValidTaxRate(decimal taxRate)
        {
            return taxRate >= 0 && taxRate <= 100;
        }
    }
}
