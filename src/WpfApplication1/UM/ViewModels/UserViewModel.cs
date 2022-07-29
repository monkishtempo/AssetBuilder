using AssetBuilder.UM.Models;

namespace AssetBuilder.UM.ViewModels
{
    public class UserViewModel : WorkspaceViewModel
    {
        private readonly AssetUser _user;

        private bool _isSelected;

        public UserViewModel(AssetUser user)
        {
            _user = user;
        }

        #region User Properties

        public int UserId
        {
            get => _user.UserId;
            set
            {
                if (value == _user.UserId) return;

                _user.UserId = value;
                base.OnPropertyChanged("UserId");
            }
        }

        public string Sprojoid
        {
            get => _user.Sprojoid;
            set
            {
                if (value == _user.Sprojoid) return;

                _user.Sprojoid = value;
                base.OnPropertyChanged("Sprojoid");
            }
        }

        public string UserName
        {
            get => _user.UserName;
            set
            {
                if (value == _user.UserName) return;

                _user.UserName = value;
                base.OnPropertyChanged("UserName");
            }
        }

        public Company Company
        {
            get => _user.Company;
            set
            {
                if (value == _user.Company) return;

                _user.Company = value;
                base.OnPropertyChanged("Company");
            }
        }

        #endregion User Properties

        #region Display Properties

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsUserSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;

                _isSelected = value;
                base.OnPropertyChanged("IsUserSelected");
            }
        }

        #endregion Display Properties
    }
}