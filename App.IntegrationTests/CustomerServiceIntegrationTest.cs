using System;
using App.CreditStrategies;
using App.Factories;
using App.Models;
using App.Repositories;
using App.Services;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace App.IntegrationTests
{
    [TestFixture]
    public class CustomerServiceIntegrationTest
    {
        private IFixture fixture;
        private Mock<ICustomerRepository> customerRepositoryMock;
        private Mock<ICompanyRepository> companyRepositoryMock;
        private Mock<ICustomerCreditServiceClientFactory> customerCreditServiceClientFactoryMock;
        private Mock<ICustomerCreditService> customerCreditServiceMock;

        private NewCustomer newCustomer;
        private Company company;
        private int creditLimit;
        private ICreditService creditService;

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());

            customerRepositoryMock = fixture.Freeze<Mock<ICustomerRepository>>();
            companyRepositoryMock = fixture.Freeze<Mock<ICompanyRepository>>();
            customerCreditServiceClientFactoryMock = fixture.Freeze<Mock<ICustomerCreditServiceClientFactory>>();
            customerCreditServiceMock = fixture.Create<Mock<ICustomerCreditService>>();
        }

        [TearDown]
        public void TearDown()
        {
            customerRepositoryMock.Reset();
            companyRepositoryMock.Reset();
            customerCreditServiceClientFactoryMock.Reset();
            customerCreditServiceMock.Reset();
        }

        [SetUp]
        public void SetUp()
        {
            newCustomer = fixture.Build<NewCustomer>()
                .With(c => c.EmailAddress, "customer@gmail.com")
                .With(c => c.DateOfBirth, DateTime.Now.AddYears(-22))
                .Create();

            company = fixture.Create<Company>();
            creditLimit = fixture.Create<int>();

            companyRepositoryMock.Setup(x => x.GetById(It.IsAny<int>())).Returns(company);
            customerCreditServiceMock.Setup(x => x.GetCreditLimit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(() => creditLimit);
            customerCreditServiceClientFactoryMock.Setup(x => x.Create()).Returns(customerCreditServiceMock.Object);

            var veryImpostantCreditStrategy = new VeryImportantClientCreditStrategy();
            var impostantCreditStrategy = new ImportantClientCreditStrategy(customerCreditServiceClientFactoryMock.Object);
            creditService = new CreditService(new ICreditStrategy[] { veryImpostantCreditStrategy, impostantCreditStrategy }, customerCreditServiceClientFactoryMock.Object);
            fixture.Inject(creditService);
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddCustomer_WhenFirstNameIsEmpty_ShouldReturnFalse(string firstName)
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            newCustomer.Firstname = firstName;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddCustomer_WhenSurnameIsEmpty_ShouldReturnFalse(string surname)
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            newCustomer.Surname = surname;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [TestCase("emailWithoutAt.com")]
        [TestCase("emailWithoutdto@com")]
        public void AddCustomer_WhenEmailIsWrong_ShouldReturnFalse(string email)
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            newCustomer.EmailAddress = email;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddCustomer_WhenAgeIsUnder21_ShouldReturnFalse()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            newCustomer.DateOfBirth = DateTime.Now.AddYears(-20);

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddCustomer_WhenVeryImportantClient_ShouldReturnTrueAndAddCustomer()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            company.Name = "VeryImportantClient";

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            customerRepositoryMock.Verify(x => x.AddCustomer(It.IsAny<Customer>()), Times.Once);
            Assert.That(result, Is.True);
        }

        [Test]
        public void AddCustomer_WhenImportantClientAndCreditLimitAtLeast500_ShouldReturnTrue()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            company.Name = "ImportantClient";
            creditLimit = 250;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AddCustomer_WhenImportantClientAndCreditLimitBelow500_ShouldReturnFalse()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            company.Name = "ImportantClient";
            creditLimit = 200;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddCustomer_WhenImportantClientAndCreditLimitAtLeast500_ShouldAddCustomerWithDoubledCreditLimit()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            company.Name = "ImportantClient";
            creditLimit = 250;

            //Act
            subject.AddCustomer(newCustomer);

            //Assert
            customerRepositoryMock.Verify(x => x.AddCustomer(It.Is<Customer>(c => c.CreditLimit == creditLimit * 2)), Times.Once);
        }

        [Test]
        public void AddCustomer_WhenNormalClientAndCreditLimitAtLeast500_ShouldReturnTrue()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            creditLimit = 500;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AddCustomer_WhenNormalClientAndCreditLimitBelow500_ShouldReturnFalse()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            creditLimit = 400;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddCustomer_WhenNormalClientAndCreditLimitAtLeast500_ShouldAddCustomerWitCreditLimit()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            creditLimit = 500;

            //Act
            subject.AddCustomer(newCustomer);

            //Assert
            customerRepositoryMock.Verify(x => x.AddCustomer(It.Is<Customer>(c => c.CreditLimit == creditLimit)), Times.Once);
        }
    }
}
