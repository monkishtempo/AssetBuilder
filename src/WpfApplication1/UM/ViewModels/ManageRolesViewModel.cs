using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AssetBuilder.Classes;
using AssetBuilder.Services;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.Services;

namespace AssetBuilder.UM.ViewModels
{
    public class ManageRolesViewModel : WorkspaceViewModel
    {
        private readonly IUserManagerRepository _repository;

        private readonly IDialogService _dialogService;

        private List<Company> _companies;

        private RelayCommand _updateCommand;

        private RelayCommand _resetPasswordCommand;

        private bool _isCompanySelected;

        private bool _isUserSelected;

        private Company _selectedCompany;

        private AssetUser _selectedUser;

        private List<UserRole> _currentRoles;

        public ManageRolesViewModel(IUserManagerRepository repository, IDialogService dialogService)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));

            base.DisplayName = "Manage Roles";
            _repository = repository;
            _dialogService = dialogService;

            PropertyChanged += OnRoleModelPropertyChanged;
        }

        #region Bindable Properties

        public List<Company> Companies
        {
            get
            {
                if (_companies == null)
                {
                    var companyList = _repository.GetCompanies();
                    if (companyList == null) return new List<Company> { new Company {Name="No companies found."}};

                    var companies = new List<Company>(companyList.Count);
                    companies.AddRange(companyList.Select(company => new Company
                    { 
                        Sprojoid = company.Sprojoid,
                        Name = company.Name
                    }));

                    _companies = companies;
                }

                return _companies;
            }
            set
            {
                _companies = value;

                base.OnPropertyChanged("Companies");
            }
        }

        public ObservableCollection<AssetUser> EnvironmentUsers { get; private set; }

        public ObservableCollection<UserRole> UsersRoles { get; set; }

        #endregion Bindable Properties

        #region Presentation Properties

        public bool IsCompanySelected
        {
            get => _isCompanySelected;
            set
            {
                if (value == _isCompanySelected) return;

                _isCompanySelected = value;

                base.OnPropertyChanged("IsCompanySelected");
            }
        }

        public Company SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                if (value == _selectedCompany) return;

                _selectedCompany = value;

                base.OnPropertyChanged("SelectedCompany");
            }
        }

        public bool IsUserSelected
        {
            get => _isUserSelected;
            set
            {
                if (value == _isUserSelected) return;

                _isUserSelected = value;

                base.OnPropertyChanged("IsUserSelected");
            }
        }

        public AssetUser SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (value == _selectedUser) return;

                _selectedUser = value;

                base.OnPropertyChanged("SelectedUser");
                base.OnPropertyChanged("AssignedRoleTitle");
                base.OnPropertyChanged("ResetPasswordTitle");
            }
        }

        public string Password
        {
            get => _selectedUser?.Password;
            set
            {
                if (value == _selectedUser.Password) return;

                _selectedUser.Password = value;
                base.OnPropertyChanged("Password");
            }
        }

        public string AssignedRoleTitle => _selectedUser == null ? "Assigned Roles" : $"Assigned Roles: {_selectedUser.UserName}";

        public string ResetPasswordTitle => _selectedUser == null ? "Reset Password" : $"Reset Password: {_selectedUser.UserName}";

        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new RelayCommand(param => UpdateRoles());
                }

                return _updateCommand;
            }
        }

        public ICommand ResetPasswordCommand
        {
            get
            {
                if (_resetPasswordCommand == null)
                {
                    _resetPasswordCommand = new RelayCommand(param => UpdatePassword(), param => CanChangePassword);
                }

                return _resetPasswordCommand;
            }
        }

        public void RefreshUsers()
        {
            if (!IsCompanySelected) return;

            // Get currently selected user
            var currentUser = _selectedUser;
            // Get updated users after user added:
            ShowUsersForCompany(_selectedCompany);
            // Reselect previously selected user (assumes not deleted)
            if (currentUser != null)
            {
                var matchingUser = EnvironmentUsers.FirstOrDefault(x => x.UserName.Equals(currentUser.UserName, StringComparison.InvariantCultureIgnoreCase));
                if (matchingUser != null && _selectedUser != matchingUser) SelectedUser = matchingUser;
            }
        }

        public override void Refresh()
        {
            var currentCompany = _selectedCompany;
            var currentUser = _selectedUser;

            // Want to clear/force reload of companies, then re-select company and user if they exist
            Companies = null;

            if (currentCompany != null)
            {
                // 'Get' on 'Companies' will force reload. Use the public properties to set - so the bindings trigger
                var matchingCompany = Companies?.FirstOrDefault(x => x.Name.Equals(currentCompany.Name, StringComparison.InvariantCultureIgnoreCase));
                if (matchingCompany != null && _selectedCompany != matchingCompany) SelectedCompany = matchingCompany;
            }

            if (currentUser != null)
            {
                var matchingUser = EnvironmentUsers.FirstOrDefault(x => x.UserName.Equals(currentUser.UserName, StringComparison.InvariantCultureIgnoreCase));
                if (matchingUser != null && _selectedUser != matchingUser) SelectedUser = matchingUser;
            }
        }

        #endregion Presentation Properties

        #region Private Helpers

        private void OnRoleModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ManageRolesViewModel model)
            {
                switch (e.PropertyName)
                {
                    case "SelectedCompany":
                        ShowUsersForCompany(model.SelectedCompany);
                        break;
                    case "SelectedUser":
                        ShowUsersRoles();
                        break;
                }
            }
        }

        private void ShowUsersForCompany(Company company)
        {
            if (company != null && !string.IsNullOrWhiteSpace(company.Sprojoid))
            {
                var all = _repository.GetCompanyUsersAndRoles(company);
                var observableCompanyUsers = new ObservableCollection<AssetUser>(all);
                EnvironmentUsers = observableCompanyUsers;
            }

            ClearUser();

            base.OnPropertyChanged("EnvironmentUsers");
        }

        private void ShowUsersRoles()
        {
            _currentRoles = new List<UserRole>();

            if (_selectedUser == null)
            {
                UsersRoles = new ObservableCollection<UserRole>(new List<UserRole>());
                base.OnPropertyChanged("UsersRoles");
                return;
            }

            UsersRoles = new ObservableCollection<UserRole>(_selectedUser.Roles);
            foreach (var role in _selectedUser.Roles)
            {
                // Value copy needed
                _currentRoles.Add(new UserRole
                {
                    Assigned = role.Assigned,
                    Id = role.Id,
                    Name = role.Name,
                    sprojoid = role.sprojoid
                });
            }

            base.OnPropertyChanged("UsersRoles");
        }

        private void ClearUser()
        {
            if (UsersRoles == null) return;

            SelectedUser = null;
            UsersRoles.Clear();
            _currentRoles.Clear();

            base.OnPropertyChanged("UsersRoles");
        }

        private void UpdateRoles()
        {
            if (_selectedUser == null) return;

            var diff = UsersRoles.Except(_currentRoles.ToList(), new UserRoleComparer()).ToList();

            var result = _repository.UpdateRolesForUser(_selectedUser, diff);

            if (result)
            {
                OnStatusChange($"Role changes for {_selectedUser.UserName} were successful.");
                _dialogService.ShowMessageBox("Role(s) updated successfully.", "Update success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                OnStatusChange($"Not all role changes for {_selectedUser.UserName} were successful. Please contact the Database Administrator.");
                _dialogService.ShowMessageBox("Not all role(s) updated successfully. Please contact the Database Administrator.", "Update failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Refresh();
        }

        private void UpdatePassword()
        {
            if (_selectedUser == null) return;

            var confirm = _dialogService.ShowMessageBox(
                $"Are you sure you want to reset the password for {_selectedUser.UserName}?", "Reset Password",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (confirm == MessageBoxResult.No) return;

            var (result, message) = _repository.PasswordReset(_selectedUser.UserName, Password);
            if (result)
            {
                OnStatusChange($"Password change for {_selectedUser.UserName} was successful.");
                _dialogService.ShowMessageBox("Password updated successfully.", "Update success", MessageBoxButton.OK, MessageBoxImage.Information);
                Password = null;
            }
            else
            {
                OnStatusChange(message);
                _dialogService.ShowMessageBox("Password change failed. Please contact the Database Administrator.", "Update failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanChangePassword => _selectedUser?.IsValid ?? false;

        #endregion Private Helpers
    }
}