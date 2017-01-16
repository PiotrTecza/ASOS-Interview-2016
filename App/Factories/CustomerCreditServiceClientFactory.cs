namespace App.Factories
{
    public class CustomerCreditServiceClientFactory : ICustomerCreditServiceClientFactory
    {
        public ICustomerCreditService Create()
        {
            return new CustomerCreditServiceClient();
        }
    }
}
