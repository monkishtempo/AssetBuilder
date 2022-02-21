using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ConclusionMap.Classes
{
    public class Rule : INotifyPropertyChanged
    {
        internal System.Action GlobalUpdate;

        private string _ruleName;
        public string ruleName { get { return _ruleName; } set { _ruleName = value; NotifyPropertyChanged("ruleName"); } }

        private ObservableCollection<Trigger> _triggers;
        public ObservableCollection<Trigger> triggers { get { return _triggers; } set { _triggers = value; NotifyPropertyChanged("triggers"); } }

        private ObservableCollection<Action> _actions;
        public ObservableCollection<Action> actions { get { return _actions; } set { _actions = value; NotifyPropertyChanged("actions"); } }

        public Rule()
        {
            triggers = new ObservableCollection<Trigger>();
            actions = new ObservableCollection<Action>();
            triggers.CollectionChanged += Triggers_CollectionChanged;
            actions.CollectionChanged += Actions_CollectionChanged;
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as Action).GlobalUpdate = GlobalUpdate;

            NotifyPropertyChanged("actions");
        }

        private void Triggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as Trigger).GlobalUpdate = GlobalUpdate;

            NotifyPropertyChanged("triggers");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            GlobalUpdate?.Invoke();
        }
    }
}
