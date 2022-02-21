using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for countIndicator.xaml
	/// </summary>
	public partial class countIndicator : UserControl
	{
		public string Number
		{
			get { return (string)GetValue(NumberProperty); }
			set { SetValue(NumberProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Number.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty NumberProperty =
			DependencyProperty.Register("Number", typeof(string), typeof(countIndicator), new UIPropertyMetadata("0", new PropertyChangedCallback(OnNumberChangedCallBack)));

		private static void OnNumberChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			countIndicator ci = sender as countIndicator;
			if (ci != null) ci.OnNumberChanged();
		}

        public bool ShowZero
        {
            get { return (bool)GetValue(ShowZeroProperty); }
            set { SetValue(ShowZeroProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowZero.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowZeroProperty =
            DependencyProperty.Register("ShowZero", typeof(bool), typeof(countIndicator), new UIPropertyMetadata(false, new PropertyChangedCallback(OnNumberChangedCallBack)));
      
		protected virtual void OnNumberChanged()
		{
            if (ShowZero || Number != "0") this.Visibility = Visibility.Visible; else Visibility = Visibility.Hidden;
		}
		
		public countIndicator()
		{
			InitializeComponent();
		}
	}
}
