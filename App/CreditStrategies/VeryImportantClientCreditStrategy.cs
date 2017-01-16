namespace App.CreditStrategies
{
    public class VeryImportantClientCreditStrategy : ICreditStrategy
    {
        public bool IsSupported(Customer customer)
        {
            return customer.Company.Name == "VeryImportantClient";
        }

        public void SetCreditData(Customer customer)
        {
            // Skip credit check
            customer.HasCreditLimit = false;
        }
    }
}
