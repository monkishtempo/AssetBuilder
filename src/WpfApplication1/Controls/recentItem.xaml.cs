using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for recentItem.xaml
	/// </summary>
	public partial class recentItem : UserControl
	{
		qcat Cat;
		public string AssetID { get; set; }
		public int AssetType { get; set; }
		public string Search { get; set; }
		public string FromID { get; set; }

		public recentItem(int assetType, string assetID, string title, string search, string fromID, qcat cat)
		{
			InitializeComponent();
			Cat = cat;
			AssetID = assetID;
			AssetType = assetType;
			Search = search;
			FromID = fromID;
			text.Text = assetID + ") " + title;
			if (search != "") img.Source = new BitmapImage(new Uri("../images/Equals.png", UriKind.Relative));
			else switch (AssetType)
				{
					case 1:
						img.Source = new BitmapImage(new Uri("../images/Algo.png", UriKind.Relative));
						break;
					case 2:
						img.Source = new BitmapImage(new Uri("../images/Question.png", UriKind.Relative));
						break;
					case 3:
						img.Source = new BitmapImage(new Uri("../images/Answer.png", UriKind.Relative));
						break;
					case 4:
						img.Source = new BitmapImage(new Uri("../images/Conclusion.png", UriKind.Relative));
						break;
					case 5:
						img.Source = new BitmapImage(new Uri("../images/Bullet.png", UriKind.Relative));
						break;
					default:
						break;
				}
		}

		private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Search != "")
			{
				Cat.AssetTypeID = AssetType;
				Window1.RadioToggle(Cat.form.assetGroup, Cat.AssetTypeID);
				Cat.LoadAssetFromList(AssetID, Search, FromID);
			}
			else
			{
				Cat.AssetTypeID = -AssetType;
				Window1.RadioToggle(Cat.form.assetGroup, Cat.AssetTypeID);
				Cat.fullLoadAsset(AssetID);
			}
		}
	}
}
