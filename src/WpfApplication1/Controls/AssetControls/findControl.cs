using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.AssetControls
{

    public class findControl : UserControl
    {
        public Button btnClose;
        public DockPanel findDockPanel;
        public FindReplace findReplace;

        static findControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(findControl), new FrameworkPropertyMetadata(typeof(findControl)));
            CloseEvent = EventManager.RegisterRoutedEvent("btnClose", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(findControl));
        }

        public findControl()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            btnClose = this.GetTemplateChild("btnClose") as Button;
            findDockPanel = this.GetTemplateChild("findDockPanel") as DockPanel;
            findReplace = this.GetTemplateChild("findReplaceControl") as FindReplace;
            (this.GetTemplateChild("btnClose") as Button).Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { OnClose(); });
        }

        public static RoutedEvent CloseEvent;

        public event RoutedEventHandler Close
        {
            add { AddHandler(CloseEvent, value); }
            remove { RemoveHandler(CloseEvent, value); }
        }

        protected virtual void OnClose()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CloseEvent;
            RaiseEvent(args);
        }

    }
}
