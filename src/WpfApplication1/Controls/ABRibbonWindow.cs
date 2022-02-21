using System.Windows.Interop;
using Microsoft.Windows.Controls.Ribbon;

namespace AssetBuilder.Controls
{
	public class ABRibbonWindow : RibbonWindow
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
