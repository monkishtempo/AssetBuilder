using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AssetBuilder
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class inherits : UserControl
	{
		public string TemplateName { get; set; }

		public ObservableCollection<string> List
		{
			get { return (ObservableCollection<string>)GetValue(ListProperty); }
			set { SetValue(ListProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ListProperty =
			DependencyProperty.Register("List", typeof(ObservableCollection<string>), typeof(inherits), new PropertyMetadata());

		public string Path
		{
			get { return (string)GetValue(PathProperty); }
			set { SetValue(PathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PathProperty =
			DependencyProperty.Register("Path", typeof(string), typeof(inherits), new UIPropertyMetadata(""));

		public string OriginalText
		{
			get { return (string)GetValue(OriginalTextProperty); }
			set { SetValue(OriginalTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for OriginalText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OriginalTextProperty =
			DependencyProperty.Register("OriginalText", typeof(string), typeof(inherits), new UIPropertyMetadata(""));

		public inherits()
		{
			InitializeComponent();
		}

		public IEnumerable<string> getCheckedLanguages()
		{
			return getLanguages(true);
		}

		public IEnumerable<string> getUncheckedLanguages()
		{
			return getLanguages(false);
		}

		private IEnumerable<string> getLanguages(bool state)
		{
			var cbs = Classes.ControlTree.getChildren<CheckBox>(listPanel);
			List<string> languages = new List<string>();
			foreach (var item in cbs)
			{
				if ((bool)item.IsChecked == state) languages.Add(item.Content.ToString());
			}
			return languages;
		}

		private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
		{
			TextBox tb = (TextBox)((Panel)VisualParent).FindName(TemplateName);
			if (tb != null && string.IsNullOrEmpty(tb.Text)) AssetBuilder.Controls.NLExtensions.textBox_AdornAndValidate(tb, null);
		}
	}
}
