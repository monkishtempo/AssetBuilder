using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Controls.Custom
{
    class CheckableItem : INotifyPropertyChanged
    {
        public CheckableItem Parent { get; set; }
        public CheckableItem Owner { get; set; }
        public CheckableItem(CheckableItem owner, CheckableItem parent)
        {
            Parent = parent;
            Owner = owner;
        }
        public ObservableCollection<CheckableItem> Children { get; set; }
        bool _IsChecked;
        public bool IsChecked { get { return _IsChecked; } set { _IsChecked = value; NotifyPropertyChanged("IsChecked"); } }
        string _Header;
        public string Header { get { return _Header; } set { _Header = value; NotifyPropertyChanged("Header"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
