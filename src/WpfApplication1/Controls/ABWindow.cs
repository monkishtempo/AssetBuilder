using System.Windows;
using System.Windows.Interop;

namespace AssetBuilder.Controls
{
	public class ABWindow : Window
	{
		bool hookadded = false;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			if (!hookadded && Window1.window != null)
			{
				HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
				source.AddHook(new HwndSourceHook(Window1.window.WndProc));
				hookadded = true;
			}
		}
	}
}
