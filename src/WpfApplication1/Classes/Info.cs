using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;

namespace AssetBuilder
{
	public class Info : MarkupExtension
	{
		public string Image { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public string Width { get; set; }

		public Info()
		{

		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			ABToolTip abt = new ABToolTip();

			if (!string.IsNullOrEmpty(Image))
			{
				abt.ABImage.Source = new BitmapImage(new Uri(Image, UriKind.Relative));
			}
			if (!string.IsNullOrEmpty(Width))
			{
				abt.MaxWidth = double.Parse(Width);
			}
			if (!string.IsNullOrEmpty(Title))
			{
				object result = Title;
				if (App.Current.Resources.Contains(Title)) result = App.Current.FindResource(Title);
				abt.ABTitle.Content = result;
			}
			if (!string.IsNullOrEmpty(Body) && !Window1.IsDesignTime)
			{
				object result = Body;
				if (App.Current.Resources.Contains(Body)) result = App.Current.FindResource(Body);
				if (result is FrameworkElement)
				{
					FrameworkElement fe = result as FrameworkElement;
					if (fe.Parent != null)
					{
						string xaml = XamlWriter.Save(result);
						FrameworkElement newfe = (FrameworkElement)XamlReader.Load(XmlReader.Create(new StringReader(xaml)));
						result = newfe;
					}
				}
				abt.ABBody.Content = result;
			}

			ToolTip tt = new ToolTip();
			tt.Content = abt;
			return tt;
		}
	}
}
