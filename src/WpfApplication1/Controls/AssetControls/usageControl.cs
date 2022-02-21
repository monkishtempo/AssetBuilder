using AssetBuilder.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AssetBuilder.AssetControls
{
    public class usageControl : UserControl
    {
        public Button btnClose;
        public TabItem btnNLAssets;
        public TabItem btnUsage;
        public TabItem btnLanguage;
        public TabItem btnProperties;
        public TabControl usageTabs;
        public bool EnableAssetSearch = true;
        public bool EnableLanguageSearch = true;
        public bool EnablePropertiesSearch = Window1.AllowProperties;
        private UsageMode _UsageMode = UsageMode.UsageMode;
        public UsageMode UsageMode
        {
            get { return _UsageMode; }
            set
            {
                _UsageMode = value;
                //if (IsUsageMode) btnUsage.Visibility = Visibility.Collapsed; else btnUsage.Visibility = Visibility.Visible;
                //if (IsAssetMode || !EnableAssetSearch) btnNLAssets.Visibility = Visibility.Collapsed; else btnNLAssets.Visibility = Visibility.Visible;
                //if (IsLanguageMode || !EnableLanguageSearch) btnLanguage.Visibility = Visibility.Collapsed; else btnLanguage.Visibility = Visibility.Visible;
                //if (IsPropertiesMode || !EnablePropertiesSearch) btnProperties.Visibility = Visibility.Collapsed; else btnProperties.Visibility = Visibility.Visible;
            }
        }
        public bool IsUsageMode { get { return UsageMode == UsageMode.UsageMode; } }
        public bool IsAssetMode { get { return UsageMode == UsageMode.AssetMode; } }
        public bool IsLanguageMode { get { return UsageMode == UsageMode.LanguageMode; } }
        public bool IsPropertiesMode { get { return UsageMode == UsageMode.PropertiesMode; } }
        public string AssetID;
        public string ConcatID = null;
        public TreeView usageTreeView;
        public DockPanel usageDockPanel;

        static usageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(usageControl), new FrameworkPropertyMetadata(typeof(usageControl)));
            CloseEvent = EventManager.RegisterRoutedEvent("btnClose", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(usageControl));
            AssetUsageEvent = EventManager.RegisterRoutedEvent("btnNLAssets", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(usageControl));
            AlgoUsageEvent = EventManager.RegisterRoutedEvent("btnUsage", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(usageControl));
            LanguageUsageEvent = EventManager.RegisterRoutedEvent("btnLanguage", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(usageControl));
            PropertiesUsageEvent = EventManager.RegisterRoutedEvent("btnProperties", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(usageControl));
        }

        public usageControl()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            btnClose = this.GetTemplateChild("btnClose") as Button;
            usageTabs = this.GetTemplateChild("usageTabs") as TabControl;
            btnNLAssets = this.GetTemplateChild("btnNLAssets") as TabItem;
            btnUsage = this.GetTemplateChild("btnUsage") as TabItem;
            btnLanguage = this.GetTemplateChild("btnLanguage") as TabItem;
            btnProperties = this.GetTemplateChild("btnProperties") as TabItem;
            //usageTreeView = this.GetTemplateChild("usageTreeView") as TreeView;
            usageDockPanel = this.GetTemplateChild("usageDockPanel") as DockPanel;

            if (!EnableAssetSearch) btnNLAssets.Visibility = Visibility.Collapsed;
            if (!EnableLanguageSearch) btnLanguage.Visibility = Visibility.Collapsed;
            if (!EnablePropertiesSearch) btnProperties.Visibility = Visibility.Collapsed;
            btnClose.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { OnClose(); });
            usageTabs.SelectionChanged += usageTabs_SelectionChanged;
            //btnUsage.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { OnAlgoUsage(); });
            //btnNLAssets.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { OnAssetUsage(); });
            //btnLanguage.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { OnLanguageUsage(); });
            //btnProperties.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { OnPropertiesUsage(); });
        }

        private void usageTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                Populate(e.AddedItems[0] as TabItem);
            }
        }

        public void Populate(TabItem ti)
        {
            var tv = ControlTree.getLogicalChild<TreeView>(ti);
            if (tv == null && Visibility == Visibility.Visible)
            {
                usageTreeView = new TreeView() { Opacity = 0.8 };
                ti.Content = usageTreeView;
                if (ti.Header.ToString() == "Algos") OnAlgoUsage();
                if (ti.Header.ToString() == "Assets") OnAssetUsage();
                if (ti.Header.ToString() == "Languages") OnLanguageUsage();
                if (ti.Header.ToString() == "Properties") OnPropertiesUsage();
            }
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
            if (args.RoutedEvent != null) RaiseEvent(args);
        }

        public static RoutedEvent AlgoUsageEvent;

        public event RoutedEventHandler AlgoUsage
        {
            add { AddHandler(AlgoUsageEvent, value); }
            remove { RemoveHandler(AlgoUsageEvent, value); }
        }

        protected virtual void OnAlgoUsage()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = AlgoUsageEvent;
            if (args.RoutedEvent != null) RaiseEvent(args);
        }

        public static RoutedEvent AssetUsageEvent;

        public event RoutedEventHandler AssetUsage
        {
            add { AddHandler(AssetUsageEvent, value); }
            remove { RemoveHandler(AssetUsageEvent, value); }
        }

        protected virtual void OnAssetUsage()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = AssetUsageEvent;
            if (args.RoutedEvent != null) RaiseEvent(args);
        }

        public static RoutedEvent LanguageUsageEvent;

        public event RoutedEventHandler LanguageUsage
        {
            add { AddHandler(LanguageUsageEvent, value); }
            remove { RemoveHandler(LanguageUsageEvent, value); }
        }

        protected virtual void OnLanguageUsage()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = LanguageUsageEvent;
            if (args.RoutedEvent != null) RaiseEvent(args);
        }

        public static RoutedEvent PropertiesUsageEvent;

        public event RoutedEventHandler PropertiesUsage
        {
            add { AddHandler(PropertiesUsageEvent, value); }
            remove { RemoveHandler(PropertiesUsageEvent, value); }
        }

        protected virtual void OnPropertiesUsage()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = PropertiesUsageEvent;
            if (args.RoutedEvent != null) RaiseEvent(args);
        }
    }

    public enum UsageMode
    {
        UsageMode,
        AssetMode,
        LanguageMode,
        PropertiesMode
    }
}
