using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.AssetControls
{
    public class nlwControl : UserControl
    {
        public Button btnClose;
        public DockPanel nlwDockPanel;
        //public NaturalLanguageWizard.NLWControl nlwc;
        //private bool setupdone = false;

        static nlwControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(nlwControl), new FrameworkPropertyMetadata(typeof(nlwControl)));
            CloseEvent = EventManager.RegisterRoutedEvent("btnClose", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(nlwControl));
        }

        public nlwControl()
        {
			
        }

        public void SetUpTextPredictors()
        {
            //if (!setupdone) nlwc.SetUpTextPredictors();
            //setupdone = true;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            btnClose = this.GetTemplateChild("btnClose") as Button;
            nlwDockPanel = this.GetTemplateChild("nlwDockPanel") as DockPanel;
            //nlwc = ((nlwDockPanel.Children[1] as ScrollViewer).Content as DockPanel).Children[0] as NaturalLanguageWizard.NLWControl;

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
