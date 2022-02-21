using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for SplitTextBox.xaml
	/// </summary>
	public partial class SplitTextBox : UserControl
	{
		public SplitTextBox()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Button b = sender as Button;
			if (b.Content.ToString() == "+")
			{
				b.Content = "-";
				txtDescription.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				b.Content = "+";
				txtDescription.Visibility = System.Windows.Visibility.Collapsed;
			}
		}
	}
}
