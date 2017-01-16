using System;
using App.Models;
using App.Repositories;
using App.Services;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace App.UnitTests
{
    [TestFixture]
    public class CustomerServiceTest
    {
        private IFixture fixture;
        private Mock<ICustomerRepository> customerRepositoryMock;
        private Mock<ICompanyRepository> companyRepositoryMock;
        private Mock<ICreditService> creditServiceMock;

        private NewCustomer newCustomer;
        private Company company;
        private bool hasCreditLimit;
        private int creditLimit;

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());

            customerRepositoryMock = fixture.Freeze<Mock<ICustomerRepository>>();
            companyRepositoryMock = fixture.Freeze<Mock<ICompanyRepository>>();
            creditServiceMock = fixture.Freeze<Mock<ICreditService>>();
        }

        [TearDown]
        public void TearDown()
        {
            customerRepositoryMock.Reset();
            companyRepositoryMock.Reset();
            creditServiceMock.Reset();
        }

        [SetUp]
        public void SetUp()
        {
            newCustomer = fixture.Build<NewCustomer>()
                .With(c => c.EmailAddress, "customer@gmail.com")
                .With(c => c.DateOfBirth, DateTime.Now.AddYears(-22))
                .Create();

            company = fixture.Create<Company>();

            companyRepositoryMock.Setup(x => x.GetById(It.IsAny<int>())).Returns(company);
            creditServiceMock.Setup(x => x.SetCreditLimit(It.IsAny<Customer>())).Callback((Customer c) =>
            {
                c.CreditLimit = creditLimit;
                c.HasCreditLimit = hasCreditLimit;
            });
        }

        [Test]
        public void AddCustomer_WhenCustomerIsNotValid_ShouldReturnFalse()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            newCustomer.Firstname = null;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddCustomer_WhenCustomerIsValid_ShouldSetCreditLimit()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();

            //Act
            subject.AddCustomer(newCustomer);

            //Assert
            creditServiceMock.Verify(x => x.SetCreditLimit(It.IsAny<Customer>()), Times.Once);
        }

        [Test]
        public void AddCustomer_WhenCustomerIsValid_ShouldAddCustomer()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            creditLimit = 600;
            hasCreditLimit = true;

            //Act
            subject.AddCustomer(newCustomer);

            //Assert
            customerRepositoryMock.Verify(
                x =>
                    x.AddCustomer(
                        It.Is<Customer>(
                            c =>
                                c.Firstname == newCustomer.Firstname 
                                && c.Surname == newCustomer.Surname 
                                && c.EmailAddress == newCustomer.EmailAddress
                                && c.DateOfBirth == newCustomer.DateOfBirth
                                && c.CreditLimit == creditLimit
                                && c.HasCreditLimit == hasCreditLimit)), Times.Once);
        }

        [Test]
        public void AddCustomer_WhenCustomerHasLimitBelow500_ShouldReturnFalse()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            hasCreditLimit = true;
            creditLimit = 400;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.False);
        }

        [TestCase(500)]
        [TestCase(600)]
        public void AddCustomer_WhenCustomerHasLimit500OrAbove_ShouldReturnTrue(int limit)
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            hasCreditLimit = true;
            creditLimit = limit;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AddCustomer_WhenCustomerHasNoLimit_ShouldReturnTrue()
        {
            //Arrange
            var subject = fixture.Create<CustomerService>();
            hasCreditLimit = false;

            //Act
            var result = subject.AddCustomer(newCustomer);

            //Assert
            Assert.That(result, Is.True);
        }
    }
}
