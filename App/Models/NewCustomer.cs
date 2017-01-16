using System;

namespace App.Models
{
    public class NewCustomer
    {
        public string Firstname { get; set; }

        public string Surname { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string EmailAddress { get; set; }

        public int CompanyId { get; set; }

        public bool Validate()
        {
            return IsEmailValid()
                && IsNameValid()
                && IsAgeValid();
        }

        private bool IsNameValid()
        {
            return !string.IsNullOrEmpty(Firstname) && !string.IsNullOrEmpty(Surname);
        }

        private bool IsEmailValid()
        {
            return EmailAddress.Contains("@") && EmailAddress.Contains(".");
        }

        private bool IsAgeValid()
        {
            var now = DateTime.Now;
            int age = now.Year - DateOfBirth.Year;
            if (now.Month < DateOfBirth.Month || (now.Month == DateOfBirth.Month && now.Day < DateOfBirth.Day)) age--;

            return age >= 21;
        }
    }
}
