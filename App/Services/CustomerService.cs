using App.Models;
using App.Repositories;
using App.Services;

namespace App
{
    public class CustomerService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly ICompanyRepository companyRepository;
        private readonly ICreditService creditService;

        public CustomerService(
            ICustomerRepository customerRepository,
            ICompanyRepository companyRepository,
            ICreditService creditService)
        {
            this.customerRepository = customerRepository;
            this.companyRepository = companyRepository;
            this.creditService = creditService;
        }

        public bool AddCustomer(NewCustomer newCustomer)
        {
            if (!newCustomer.Validate())
            {
                return false;
            }

            var company = companyRepository.GetById(newCustomer.CompanyId);

            var customer = new Customer
                               {
                                   Company = company,
                                   DateOfBirth = newCustomer.DateOfBirth,
                                   EmailAddress = newCustomer.EmailAddress,
                                   Firstname = newCustomer.Firstname,
                                   Surname = newCustomer.Surname
                               };

            creditService.SetCreditLimit(customer);

            if (customer.HasCreditLimit && customer.CreditLimit < 500)
            {
                return false;
            }

            customerRepository.AddCustomer(customer);

            return true;
        }
    }
}
