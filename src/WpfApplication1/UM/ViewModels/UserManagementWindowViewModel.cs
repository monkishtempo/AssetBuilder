using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AssetBuilder.Classes;
using AssetBuilder.Services;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.Services;
using AssetBuilder.ViewModels;

namespace AssetBuilder.UM.ViewModels
{
    public class UserManagementWindowViewModel : WorkspaceViewModel
    {
        private readonly UserSecurity _currentUser;

        private readonly IUserManagerRepository _repository;

        private readonly IDialogService _dialogService;

        private ReadOnlyCollection<CommandViewModel> _commands;

        private ObservableCollection<WorkspaceViewModel> _workspaces;
        
        private string _activeWorkspaceName;

        private bool _disposed;

        private RelayCommand _globalSync;

        private RelayCommand _dbSetup;

        public UserManagementWindowViewModel(string currentUserName)
        {
            if (string.IsNullOrWhiteSpace(currentUserName)) throw new ArgumentNullException(nameof(currentUserName));

            _currentUser = new UserSecurity(currentUserName);
            _repository = new UserManagerRepository(_currentUser);
            _dialogService = new DialogService();

            _repository.UserAdded += OnUserAdded;

            base.DisplayName = "Admin: User Management";
        }

        public string ActiveWorkspaceName
        {
            get => _activeWorkspaceName;
            set
            {
                if (!string.IsNullOrWhiteSpace(_activeWorkspaceName) && _activeWorkspaceName == value) return;

                _activeWorkspaceName = value;
                base.OnPropertyChanged("ActiveWorkspaceName");
            }
        }

        public string StatusMessage { get; private set; }

        #region Workspaces

        // Bound in the UI
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                }

                return _workspaces;
            }
        }
        
        #endregion Workspaces

        #region Commands
        
        // Bound in the UI
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    var commands = CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(commands);

                    ShowMyProfile();
                }

                return _commands;
            }
        }

        private List<CommandViewModel> CreateCommands()
        {
            if (_currentUser.IsServerAdmin || _currentUser.IsAssetBuilderAdmin)
            {
                return new List<CommandViewModel>
                {
                    new CommandViewModel("My Profile", new RelayCommand(param => ShowMyProfile())),
                    new CommandViewModel("Add New User", new RelayCommand(param => AddUser())),
                    new CommandViewModel("Manage Roles", new RelayCommand(param => ManageRoles())),
                    new CommandViewModel("Company Assignment", new RelayCommand(param => EntityAssignment()))
                };
            }

            return new List<CommandViewModel>
            {
                new CommandViewModel("My Profile", new RelayCommand(param => ShowMyProfile())),
            };
        }

        public ICommand GlobalSyncCommand
        {
            get { return _globalSync ?? (_globalSync = new RelayCommand(param => GlobalSync())); }
        }

        public ICommand InitDatabase
        {
            get { return _dbSetup ?? (_dbSetup = new RelayCommand(param => InitialiseDatabase())); }
        }
        
        #endregion Commands

        #region Private Helpers

        private void GlobalSync()
        {
            var (result, initialised) = _repository.SynchroniseUsers();
            if (!initialised)
            {
                const string msg = "Database requires initialisation before synchronisation can take place.";
                OnStatusChanged(this, new StatusEventArgs(msg));
                _dialogService.ShowMessageBox(msg, "Synchronisation failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!result)
            {
                const string msg = "Synchronisation failed, please contact the Database Administrator.";
                OnStatusChanged(this, new StatusEventArgs(msg));
                _dialogService.ShowMessageBox(msg, "Synchronisation failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            const string message = "Synchronisation complete.";
            OnStatusChanged(this, new StatusEventArgs(message));
            _dialogService.ShowMessageBox(message, "Synchronisation successful", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshWorkspaces();
        }

        private void InitialiseDatabase()
        {
            var result = _repository.SetupDatabase();
            var message = result
                ? "Database initialisation complete."
                : "Database initialisation failed, please contact the Database Administrator.";

            if (result)
            {
                _dialogService.ShowMessageBox(message, "Initialisation successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _dialogService.ShowMessageBox(message, "Synchronisation failed.", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnStatusChanged(this, new StatusEventArgs(message));
        }

        private void ShowMyProfile()
        {
            var workspace = Workspaces.FirstOrDefault(vm => vm is UserProfileViewModel) as UserProfileViewModel;

            if (workspace == null)
            {
                workspace = new UserProfileViewModel(_repository);
                workspace.StatusChanged += OnStatusChanged;
                Workspaces.Add(workspace);
            }

            SetActiveWorkspace(workspace);
        }

        private void AddUser()
        {
            var workspace = Workspaces.FirstOrDefault(vm => vm is NewUserViewModel) as NewUserViewModel;

            if (workspace == null)
            {
                workspace = new NewUserViewModel(AssetUser.CreateNewUser(), _repository, _dialogService);
                workspace.StatusChanged += OnStatusChanged;
                Workspaces.Add(workspace);
            }

            SetActiveWorkspace(workspace);
        }

        private void ManageRoles()
        {
            var workspace = Workspaces.FirstOrDefault(vm => vm is ManageRolesViewModel) as ManageRolesViewModel;

            if (workspace == null)
            {
                workspace = new ManageRolesViewModel(_repository, _dialogService);
                workspace.StatusChanged += OnStatusChanged;
                Workspaces.Add(workspace);
            }

            SetActiveWorkspace(workspace);
        }

        private void EntityAssignment()
        {
            var workspace = Workspaces.FirstOrDefault(vm => vm is EntityAssignmentViewModel) as EntityAssignmentViewModel;

            if (workspace == null)
            {
                workspace = new EntityAssignmentViewModel(_repository, _dialogService);
                workspace.StatusChanged += OnStatusChanged;
                Workspaces.Add(workspace);
            }

            SetActiveWorkspace(workspace);
        }

        private void SetActiveWorkspace(ViewModelBase workspace)
        {
            var collectionView = CollectionViewSource.GetDefaultView(Workspaces);
            if (collectionView != null)
            {
                collectionView.MoveCurrentTo(workspace);
                ActiveWorkspaceName = workspace.DisplayName;

                OnStatusChanged(this, new StatusEventArgs(string.Empty));
            }
        }

        private void OnStatusChanged(object sender, StatusEventArgs e)
        {
            StatusMessage = e.Status;
            base.OnPropertyChanged("StatusMessage");
        }

        private void OnUserAdded(object sender, UserAddedEventArgs e)
        {
            RefreshWorkspaces();
        }

        private void RefreshWorkspaces()
        {
            foreach (var workspace in Workspaces)
            {
                workspace.Refresh();
            }
        }

        #endregion Private Helpers

        public virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            foreach (var workspace in Workspaces)
            {
                workspace.StatusChanged -= OnStatusChanged;
            }

            _repository.UserAdded -= OnUserAdded;

            _disposed = true;
        }

        public new void Dispose()
        {
            Dispose(true);
        }
    }
}