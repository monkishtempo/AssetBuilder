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
    public class EntityAssignmentViewModel : WorkspaceViewModel
    {
        private readonly IUserManagerRepository _repository;

        private readonly IDialogService _dialogService;

        private List<Company> _companies;

        private bool _isCompanySelected;

        private Company _selectedCompany;

        private Company _destinationCompany;

        private RelayCommand _updateCommand;

        public EntityAssignmentViewModel(IUserManagerRepository repository, IDialogService dialogService)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));

            base.DisplayName = "Company Assignment";
            _repository = repository;
            _dialogService = dialogService;

            PropertyChanged += OnEntityAssignmentModelPropertyChanged;
        }

        #region Bindable Properties

        public List<Company> Companies
        {
            get
            {
                if (_companies == null)
                {
                    var companyList = _repository.GetCompanies();
                    if (companyList == null) return new List<Company> { new Company { Name = "No companies found." } };

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

        public ObservableCollection<UserViewModel> EnvironmentUsers { get; private set; }

        public Company DestinationCompany
        {
            get => _destinationCompany;
            set
            {
                if (value == _destinationCompany || value == null) return;

                _destinationCompany = value;

                base.OnPropertyChanged("DestinationCompany");
            }
        }

        #endregion Bindable Properties

        #region Presentation Properties

        public Company SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                if (value == _selectedCompany || value == null) return;

                _selectedCompany = value;

                base.OnPropertyChanged("SelectedCompany");
            }
        }

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

        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new RelayCommand(param => UpdateUsersCompany());
                }

                return _updateCommand;
            }
        }

        public override void Refresh()
        {
            // Want to clear/force reload of companies, then re-select company
            var currentCompany = _selectedCompany;
            Companies = null;

            if (currentCompany != null)
            {
                // 'Get' on 'Companies' will force reload. Use the public properties to set - so the bindings trigger
                var matchingCompany = Companies?.FirstOrDefault(x => x.Name.Equals(currentCompany.Name, StringComparison.InvariantCultureIgnoreCase));
                if (matchingCompany != null && _selectedCompany != matchingCompany) SelectedCompany = matchingCompany;
            }
        }

        #endregion Presentation Properties

        #region Private Helpers
        private void OnEntityAssignmentModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is EntityAssignmentViewModel model)
            {
                switch (e.PropertyName)
                {
                    case "SelectedCompany":
                        ShowUsersForCompany(model.SelectedCompany);
                        break;
                }
            }
        }

        private void ShowUsersForCompany(Company company)
        {
            var all = _repository.GetCompanyUsers(company).Select( x => new UserViewModel(x));
            var observableCompanyUsers = new ObservableCollection<UserViewModel>(all);
            EnvironmentUsers = observableCompanyUsers;

            base.OnPropertyChanged("EnvironmentUsers");
        }

        private void UpdateUsersCompany()
        {
            if (SelectedCompany == null)
            {
                OnStatusChange("No source company/user specified.");
                return;
            }

            if (DestinationCompany == null)
            {
                OnStatusChange("No destination company specified.");
                return;
            }

            if (DestinationCompany.Name.Equals(SelectedCompany.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                OnStatusChange("Destination company is the same as the original company.");
                return;
            }

            var selectedUsers = EnvironmentUsers.Where(vm => vm.IsUserSelected).ToList();
            if (!selectedUsers.Any())
            {
                OnStatusChange("No users selected.");
                return;
            }

            var overallResult = true;
            foreach (var selectedUser in selectedUsers)
            {
                var result = _repository.MoveUserToCompany(selectedUser.Sprojoid, DestinationCompany.Sprojoid);
                overallResult = overallResult && result;
            }

            ShowSaveResult(overallResult);
        }

        private void ShowSaveResult(bool result)
        {
            if (result)
            {
                foreach (var user in EnvironmentUsers)
                {
                    user.IsUserSelected = false;
                }
                ShowUsersForCompany(SelectedCompany);
                _dialogService.ShowMessageBox("User(s) updated successfully.", "Update Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OnStatusChange("Not all changes were successful. Please contact the Database Administrator.");
            _dialogService.ShowMessageBox("Change of company for the selected user(s) was unsuccessful.", "Update failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion Private Helpers
    }
}