using System;
using System.ComponentModel;

namespace ConclusionMap.Classes
{
	public class Trigger : INotifyPropertyChanged
	{
		internal System.Action GlobalUpdate;

		private string _propertyName;
		public string propertyName { get { return _propertyName; } set { _propertyName = value; NotifyPropertyChanged("propertyName"); } }

		private string _op;
		public string op { get { return _op; } set { _op = value; NotifyPropertyChanged("op"); } }

		private string _value;
		public string value { get { return _value; } set { _value = value; NotifyPropertyChanged("value"); } }

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			GlobalUpdate?.Invoke();
		}
	}
}
