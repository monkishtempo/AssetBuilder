using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for richPopup.xaml
	/// </summary>
	public partial class richPopup : Popup
	{
		public static RoutedEvent OpenAssetEvent = EventManager.RegisterRoutedEvent("OpenAsset", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(richPopup));

		public event RoutedEventHandler OpenAsset
		{
			add { AddHandler(OpenAssetEvent, value); }
			remove { RemoveHandler(OpenAssetEvent, value); }
		}

		protected virtual void OnOpenAsset()
		{
			RoutedEventArgs args = new RoutedEventArgs();
			args.RoutedEvent = OpenAssetEvent;
			RaiseEvent(args);
		}

		public richPopup(string t, bool showLink) : this(t)
		{
			if (!showLink)
			{
				link.Visibility = System.Windows.Visibility.Collapsed;
				assetText.Margin = new Thickness(0, 0, 10, 0);
			}
		}

		public richPopup(string t)
		{
			InitializeComponent();
			Placement = PlacementMode.Mouse;
			AllowsTransparency = true;
			assetText.Text = t;
		}

		private void Thumb_MouseEnter(object sender, MouseEventArgs e)
		{
			Thumb thumb = (Thumb)sender;
			thumb.Cursor = Cursors.Hand;
		}

		private void Thumb_MouseLeave(object sender, MouseEventArgs e)
		{
			Thumb thumb = (Thumb)sender;
			thumb.Cursor = null;
		}

		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			RaiseEvent(new RoutedEventArgs { RoutedEvent = OpenAssetEvent });
		}

		private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
		{
			IsOpen = false;
		}

		private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			this.HorizontalOffset += e.HorizontalChange;
			this.VerticalOffset += e.VerticalChange;
		}
	}
}
