using System;
using System.Collections.ObjectModel;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.Services;

namespace AssetBuilder.UM.ViewModels
{
    public class UserProfileViewModel : WorkspaceViewModel
    {
        private readonly IUserManagerRepository _repository;

        private UserInfoSessionLoginHistory _loginHistory;
        
        #region Bindable Properties

        public string AccountName
        {
            get => _loginHistory.DomainLoginAccount;
            set
            {
                if (value == _loginHistory.DomainLoginAccount) return;

                _loginHistory.DomainLoginAccount = value;
                base.OnPropertyChanged("AccountName");
            }
        }

        public DateTime? CurrentLogin
        {
            get => _loginHistory.Current;
            set
            {
                if (value == _loginHistory.Current) return;

                _loginHistory.Current = value;
                base.OnPropertyChanged("CurrentLogin");
            }
        }

        public DateTime? PreviousLogin
        {
            get => _loginHistory.Previous;
            set
            {
                if (value == _loginHistory.Previous) return;

                _loginHistory.Previous = value;
                base.OnPropertyChanged("PreviousLogin");
            }
        }

        public DateTime? FormerLogin
        {
            get => _loginHistory.Former;
            set
            {
                if (value == _loginHistory.Former) return;

                _loginHistory.Former = value;
                base.OnPropertyChanged("FormerLogin");
            }
        }

        public UserRoleAssignment AssignedRoles { get; set; }

        public ReadOnlyObservableCollection<UserRoleAssignmentHistory> RoleAssignmentHistory { get; private set; }

        public ReadOnlyObservableCollection<UserAudit> RoleAuditHistory { get; private set; }

        public ReadOnlyObservableCollection<UserAudit> RolePersonalAuditHistory { get; private set; }

        #endregion Bindable Properties

        public UserProfileViewModel(IUserManagerRepository repository)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            base.DisplayName = "My profile";
            _repository = repository;

            Populate();
        }

        #region Private Helpers

        private void Populate()
        {
            PopulateSessionDetails();
            PopulateAssignedRoles();
            PopulateRoleHistory();
            PopulateAuditHistory();
            PopulatePersonalAuditHistory();
        }

        private void PopulateSessionDetails()
        {
            _loginHistory = _repository.GetLoginHistory() ?? new UserInfoSessionLoginHistory();
        }

        private void PopulateAssignedRoles()
        {
            AssignedRoles = _repository.GetRoleAssignment();
        }

        private void PopulatePersonalAuditHistory()
        {
            var all = _repository.GetRoleAssignmentHistory();
            var observableAll = new ObservableCollection<UserRoleAssignmentHistory>(all);
            RoleAssignmentHistory = new ReadOnlyObservableCollection<UserRoleAssignmentHistory>(observableAll);
        }

        private void PopulateAuditHistory()
        {
            var all = _repository.GetAuditHistory();
            var observableAll = new ObservableCollection<UserAudit>(all);
            RoleAuditHistory = new ReadOnlyObservableCollection<UserAudit>(observableAll);
        }

        private void PopulateRoleHistory()
        {
            var all = _repository.GetPersonalAuditHistory();
            var observableAll = new ObservableCollection<UserAudit>(all);
            RolePersonalAuditHistory = new ReadOnlyObservableCollection<UserAudit>(observableAll);
        }

        #endregion Private Helpers
    }
}