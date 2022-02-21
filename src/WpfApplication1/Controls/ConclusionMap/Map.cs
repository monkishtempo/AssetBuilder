using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Web.Script.Serialization;

namespace ConclusionMap.Classes
{
    public class Map : INotifyPropertyChanged
    {
        internal System.Action GlobalUpdate;


        private int _id;
        public int id { get { return _id; } set { _id = value; NotifyPropertyChanged("id"); } }

        private string _name;
        public string name { get { return _name; } set { _name = value; NotifyPropertyChanged("name"); } }

        private string _releaseNumber;
        public string releaseNumber { get { return _releaseNumber; } set { _releaseNumber = value; NotifyPropertyChanged("releaseNumber"); } }

        private ObservableCollection<Rule> _rules;
        public ObservableCollection<Rule> rules { get { return _rules; } set { _rules = value; NotifyPropertyChanged("rules"); } }

        private bool _EditMode;
        [ScriptIgnore]
        public bool EditMode { get { return _EditMode; } set { _EditMode = value; NotifyPropertyChanged("EditMode"); } }

        public Map()
        {
            rules = new ObservableCollection<Rule>();
            rules.CollectionChanged += Rules_CollectionChanged;            
        }

        private void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as Rule).GlobalUpdate = GlobalUpdate;

            NotifyPropertyChanged("rules");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            GlobalUpdate?.Invoke();
        }
    }
}
