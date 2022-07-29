using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AssetBuilder.Classes;
using AssetBuilder.Services;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.Services;

namespace AssetBuilder.UM.ViewModels
{
    public class NewUserViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        private readonly IUserManagerRepository _repository;

        private readonly IDialogService _dialogService;

        private RelayCommand _saveCommand;

        private AssetUser _newUser;

        private List<Company> _companyOptions;

        public NewUserViewModel(AssetUser user, IUserManagerRepository repository, IDialogService dialogService)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));

            base.DisplayName = "Add new user";
            _newUser = user;
            _repository = repository;
            _dialogService = dialogService;
        }

        #region Bindable Properties

        public string UserName
        {
            get => _newUser.UserName;
            set
            {
                if (value == _newUser.UserName) return;

                _newUser.UserName = value;
                base.OnPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get => _newUser.Password;
            set
            {
                if (value == _newUser.Password) return;

                _newUser.Password = value;
                base.OnPropertyChanged("Password");
            }
        }

        public Company UserCompany
        {
            get => _newUser.Company;
            set
            {
                if (value == _newUser.Company || value == null) return;

                _newUser.Company = value;
                base.OnPropertyChanged("UserCompany");
            }
        }
        
        public List<Company> CompanyOptions
        {
            get
            {
                if (_companyOptions == null)
                {
                    var companyList = _repository.GetCompanies();
                    if (companyList == null) return new List<Company>{ new Company { Name = "No companies found."}};

                    var companies = new List<Company>(companyList.Count);
                    companies.AddRange(companyList);

                    _companyOptions = companies;
                }

                return _companyOptions;
            }
        }

        #endregion Bindable Properties

        #region Presentation Properties

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(param => Save(), param => CanSave);
                }

                return _saveCommand;
            }
        }

        #endregion Presentation Properties

        #region Private Helpers

        private void Save()
        {
            if (!_newUser.IsValid) throw new InvalidOperationException("Unable to save, user details are not valid.");

            var (result, message) = _repository.AddUser(_newUser);
            if (result)
            {
                base.OnStatusChange($"New user '{_newUser.UserName}' saved successfully.");
                _dialogService.ShowMessageBox("User successfully added.", "Update success", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearUser();
            }
            else
            {
                base.OnStatusChange(message);
                _dialogService.ShowMessageBox("Failed to add user to the database. Please contact the Database Administrator.", "Update failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSave => _newUser.IsValid;

        private void ClearUser()
        {
            _newUser = AssetUser.CreateNewUser();
            base.OnPropertyChanged("UserName");
            base.OnPropertyChanged("Password");
            base.OnPropertyChanged("UserCompany");
        }

        #endregion Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error => (_newUser as IDataErrorInfo).Error;

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                var error = propertyName == "UserCompany" ? ValidateCompanyOption() : (_newUser as IDataErrorInfo)[propertyName];

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        private string ValidateCompanyOption()
        {
            if (UserCompany == null || UserCompany.Name == "No companies found." || UserCompany.Name == "(Select a company)")
            {
                return "No company selected.";
            }

            return null;
        }

        #endregion IDataErrorInfo Members
    }
}