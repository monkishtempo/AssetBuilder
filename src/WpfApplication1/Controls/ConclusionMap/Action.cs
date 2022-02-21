using System;
using System.ComponentModel;

namespace ConclusionMap.Classes
{
	public class Action : INotifyPropertyChanged
	{
		internal System.Action GlobalUpdate;

		private string _verb;
		public string verb { get { return _verb; } set { _verb = value; NotifyPropertyChanged("verb"); } }

		private string _propertyName;
		public string propertyName { get { return _propertyName; } set { _propertyName = value; NotifyPropertyChanged("propertyName"); } }

		private string _value;
		public string value { get { return _value; } set { _value = value; NotifyPropertyChanged("value"); } }

		private string _replace;
		public string replace { get { return _replace; } set { _replace = value; NotifyPropertyChanged("replace"); } }

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			GlobalUpdate?.Invoke();
		}
	}
}
