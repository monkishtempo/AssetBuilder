using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AssetBuilder.UM.Models
{
    public class AssetUser : IDataErrorInfo
    {
        private static readonly string[] ValidatedProperties =
        {
            "UserName",
            "Password",
            "Company"
        };

        public static AssetUser CreateNewUser()
        {
            return new AssetUser();
        }

        public AssetUser()
        {
        }

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string propertyName] => GetValidationError(propertyName);

        #endregion IDataErrorInfo Members

        public int UserId { get; set; }

        public string Sprojoid { get; set; }

        public Company Company { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Status { get; set; }

        public List<UserRole> Roles { get; set; }

        public int ActiveRoleCount => Roles.Count(x => x.Assigned);

        public bool IsValid
        {
            get
            {
                return ValidatedProperties.All(property => GetValidationError(property) == null);
            }
        }

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0) return null;

            string error = null;
            switch (propertyName)
            {
                case "UserName":
                    error = ValidateUserName();
                    break;
                case "Password":
                    error = ValidatePassword();
                    break;
                case "Company":
                    error = ValidateCompany();
                    break;
            }

            return error;
        }

        private string ValidateUserName()
        {
            return IsStringMissing(UserName) ? "User name must be supplied." : null;
        }

        private string ValidatePassword()
        {
            const int minLength = 12;
            if (IsStringMissing(Password)) return "Password must be supplied.";

            return Password.Trim().Length < minLength ? $"Password must be at least {minLength} characters long." : null;
        }

        private string ValidateCompany()
        {
            if (Company == null) return "A Company name must be supplied.";

            if (string.IsNullOrWhiteSpace(Company.Name) || Company.Name == "No companies found.") return "A valid Company name must be supplied."; // TODO: Define default strings

            return null;
        }

        private static bool IsStringMissing(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim() == string.Empty;
        }
    }
}