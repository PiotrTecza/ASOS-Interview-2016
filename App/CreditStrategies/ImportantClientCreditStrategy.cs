using App.Factories;

namespace App.CreditStrategies
{
    public class ImportantClientCreditStrategy : ICreditStrategy
    {
        private readonly ICustomerCreditServiceClientFactory customerCreditServiceClientFactory;

        public ImportantClientCreditStrategy(ICustomerCreditServiceClientFactory customerCreditServiceClientFactory)
        {
            this.customerCreditServiceClientFactory = customerCreditServiceClientFactory;
        }

        public bool IsSupported(Customer customer)
        {
            return customer.Company.Name == "ImportantClient";
        }

        public void SetCreditData(Customer customer)
        {
            // Do credit check and double credit limit
            customer.HasCreditLimit = true;
            using (var customerCreditService = customerCreditServiceClientFactory.Create())
            {
                var creditLimit = customerCreditService.GetCreditLimit(customer.Firstname, customer.Surname, customer.DateOfBirth);
                creditLimit = creditLimit * 2;
                customer.CreditLimit = creditLimit;
            }
        }
    }
}
