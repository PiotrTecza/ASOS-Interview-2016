using System.Linq;
using App.CreditStrategies;
using App.Factories;

namespace App.Services
{
    public class CreditService : ICreditService
    {
        private readonly ICustomerCreditServiceClientFactory customerCreditServiceClientFactory;
        private readonly ICreditStrategy[] creditStrategies;

        public CreditService(ICreditStrategy[] creditStrategies, ICustomerCreditServiceClientFactory customerCreditServiceClientFactory)
        {
            this.customerCreditServiceClientFactory = customerCreditServiceClientFactory;
            this.creditStrategies = creditStrategies;
        }

        public void SetCreditLimit(Customer customer)
        {
            var strategy = creditStrategies.FirstOrDefault(x => x.IsSupported(customer));
            if (strategy == null)
            {
                // Do credit check
                customer.HasCreditLimit = true;
                using (var customerCreditService = customerCreditServiceClientFactory.Create())
                {
                    var creditLimit = customerCreditService.GetCreditLimit(customer.Firstname, customer.Surname, customer.DateOfBirth);
                    customer.CreditLimit = creditLimit;
                }
            }
            else
            {
                strategy.SetCreditData(customer);
            }
        }
    }
}
