using AssetBuilder.Controls;
using AssetBuilder.Properties;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using AssetBuilder.Classes;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Control = System.Windows.Controls.Control;
using DataGrid = System.Windows.Controls.DataGrid;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;
using WebBrowser = System.Windows.Controls.WebBrowser;
using System.Windows.Media;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Threading.Tasks;
using AssetBuilder.UM.ViewModels;
using AssetBuilder.UM.Views;
using AssetBuilder.ViewModels;
using ListItem = AssetBuilder.ViewModels.ListItem;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : ABRibbonWindow
    {
        private const int RF_TESTMESSAGE = 0xA123;

        private UserManagementWindow _userManagementWindow;

        public static SecurityContext Security { get; set; }

        public ObservableCollection<string> AlternateLanguages
        {
            get { return (ObservableCollection<string>)GetValue(AlternateLanguageProperty); }
            set { SetValue(AlternateLanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AlternateLanguage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlternateLanguageProperty =
            DependencyProperty.Register("AlternateLanguages", typeof(ObservableCollection<string>), typeof(Window1), new UIPropertyMetadata());

        public ObservableCollection<string> TranslationLanguages
        {
            get { return (ObservableCollection<string>)GetValue(TranslationLanguageProperty); }
            set { SetValue(TranslationLanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AlternateLanguage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TranslationLanguageProperty =
            DependencyProperty.Register("TranslationLanguages", typeof(ObservableCollection<string>), typeof(Window1), new UIPropertyMetadata());

        public string MyProperty
        {
            get { return (string)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(string), typeof(Window1), new UIPropertyMetadata("Hello"));



        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static UserSecurityLevel UserLevel { get; set; }
        private UserSecurityLevel PreviousUserLevel { get; set; }
        public static int StartupID { get; set; }
        public static int StartupType { get; set; }
        private static bool _LoggedIn;
        public static bool LoggedIn
        {
            get { return _LoggedIn; }
            set
            {
                _LoggedIn = value;
                if (StartupType > 0 && StartupID > 0 && _LoggedIn == true)
                {
                    window.OpenAsset(StartupType, StartupID.ToString());
                    StartupID = 0;
                    StartupType = 0;
                }
            }
        }
        public static System.Windows.Markup.XmlLanguage DefaultLanguage { get; set; }
        public static Window1 window;
        public static string windowTitle;
        public static RibbonGalleryCategory RecentAssets;
        public static bool MultiTextLanguage = false;
        public static bool AssetMapping = false;
        public static bool AllowGroups = false;
        public static bool AllowProperties = false;
        public static bool AllowTextAssets = false;
        public static bool AllowAudit = false;
        public static bool AllowRiskCalculator = false;
        public static bool AllowGraph = false;
        public static bool AllowTableEdit = false;
        public static bool AllowConclusionMap = false;
        public static bool AllowSaaSIntegration = false;
        public static bool AssetListIcons = false;
        public static bool AllowExportReport;
        public static bool AllowReleaseStatusView;
        public static int RiskMotherAlgo = 2530;
        public static bool McKesson_Mode = false; // Properties.Settings.Default.McKesson_Mode;
        public static WebView2 TraversalClient;
        public static WebView2 TraversalFinder;

        public static bool IsReviewerOrEditor
        {
            get { return UserLevel == UserSecurityLevel.Reviewer || UserLevel == UserSecurityLevel.Editor || UserLevel == UserSecurityLevel.Translator; }
        }

        public static bool IsBuilderOrAdmin
        {
            get { return UserLevel == UserSecurityLevel.Builder || UserLevel == UserSecurityLevel.Admin || Security == SecurityContext.Open; }
        }

        public static bool IsTranslator
        {
            get { return UserLevel == UserSecurityLevel.Translator; }
        }

        public static bool IsReviewer
        {
            get { return UserLevel == UserSecurityLevel.Reviewer; }
        }

        public static bool IsReviewerOrTranslator
        {
            get { return UserLevel == UserSecurityLevel.Reviewer || UserLevel == UserSecurityLevel.Translator; }
        }

        public static bool IsEditor
        {
            get { return UserLevel == UserSecurityLevel.Editor || (UserLevel == UserSecurityLevel.Translator && Window1.EditTranslation && window.TranslationLanguages.Contains(TranslationLanguage, StringComparer.OrdinalIgnoreCase)); }
        }

        public static bool CanMoveAssets
        {
            get { return !IsReviewerOrEditor && CategoryEnabled; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            setTitle();
        }

        private static void saveSettings()
        {
            Settings.Default.Save();
            setTitle();
        }

        private static void setTitle()
        {
            window.Title = "Asset Builder";
            Program.AssetBuilderTitle = null;
            window.Title = Program.AssetBuilderTitle;
            if (UserLevel != UserSecurityLevel.Unknown) window.Title += " " + UserLevel.ToString();
            windowTitle = window.Title;
            window.btnAlgoManagement.IsEnabled = UserLevel == UserSecurityLevel.Admin || Security == SecurityContext.Open;
            window.btnTableEdit.IsEnabled = UserLevel == UserSecurityLevel.Admin || Security == SecurityContext.Open;
            window.btnUserManagement.IsEnabled = UserLevel == UserSecurityLevel.Admin;
            window.rtbAutoSave.IsEnabled = UserLevel != UserSecurityLevel.Reviewer;
            window.btnUpdateDervivedAssets.IsEnabled = UserLevel != UserSecurityLevel.Reviewer;
            window.btnDebugApplication.IsEnabled = UserLevel == UserSecurityLevel.Admin || Security == SecurityContext.Open;
            window.AlternateLanguages = new ObservableCollection<string>(window.qcat1.Defaults.SelectNodes("//*/AlternateLanguage").OfType<XmlNode>().Select(f => f.InnerText));
            window.TranslationLanguages = new ObservableCollection<string>(window.qcat1.Defaults.SelectNodes("//*/TranslationLanguage").OfType<XmlNode>().Select(f => f.InnerText));
            var server = qcat.BuilderDefaults.SelectSingleNode("*/Feature[starts-with(., 'ServerLocation')]");
            window.ServerLocationBar.Visibility = server == null ? Visibility.Collapsed : Visibility.Visible;
            if (server != null)
            {
                var bc = new BrushConverter();
                var split = server.InnerText.Split('|');
                if (split.Length > 1) window.ServerLocationName.Text = split[1];
                if (split.Length > 2) window.ServerLocationBar.SetValue(BackgroundProperty, bc.ConvertFrom(split[2]));
                if (split.Length > 3) window.ServerLocationName.SetValue(ForegroundProperty, bc.ConvertFrom(split[3]));
            }
        }

        public void OpenAsset(int AssetTypeID, string AssetID)
        {
            try
            {
                if (this.WindowState == System.Windows.WindowState.Minimized) this.WindowState = System.Windows.WindowState.Normal;
                this.Topmost = true;
                this.Topmost = false;
                this.Focus();
                //if (qcat1.AssetTypeID != AssetTypeID)
                qcat1.AssetTypeId = -AssetTypeID;
                if (qcat1.AssetTypeId == AssetTypeID)
                {
                    RadioToggle(assetGroup, qcat1.AssetTypeId);
                    qcat1.FullLoadAsset(AssetID);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error caught in message handling", ex);
            }
            finally
            {
                loadingAsset = false;
            }
        }

        bool loadingAsset = false;

        [DebuggerStepThrough()]
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages here
            if (msg == RF_TESTMESSAGE && !loadingAsset)
            {
                loadingAsset = true;
                handled = true;
                int AssetType = lParam.ToInt32();
                string AssetID = wParam.ToString();
                OpenAsset(wParam.ToString(), lParam.ToInt32());
            }
            return IntPtr.Zero;
        }

        private void OpenAsset(string AssetID, int AssetTypeID)
        {
            this.Dispatcher.BeginInvoke(new Action<int, string>(OpenAsset), DispatcherPriority.Normal, AssetTypeID, AssetID);
        }

        static Label statusBarLabel;
        public static bool EnableLanguageInheritance { get; set; }
        public static bool CollapseDisabled { get; set; }
        public static bool AutoSave { get; set; }
        public static bool DisableComments { get; set; }
        public static bool DisableSpelling { get; set; }
        public static string TranslationLanguage { get; set; }
        public static bool ShowTranslation { get { return !string.IsNullOrEmpty(TranslationLanguage); } }
        public static bool SearchTranslation { get; set; }
        public static bool EditTranslation { get; set; }
        public static bool DisableValidation { get; set; }
        public static bool DisableHTMLValidation { get; set; }
        public static bool PriorityEnabled { get; set; }
        public static bool CategoryEnabled { get; set; }
        public static Action EmptyDelegate = delegate () { };
        public static AssetBuilder.Controls.NLInfo NLI;

        internal static void setStatus(string content)
        {
            try
            {
                if (statusBarLabel != null && (statusBarLabel.Content == null || statusBarLabel.Content.ToString() != content))
                {
                    statusBarLabel.Content = content;
                    statusBarLabel.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
                }
            }
            catch { }
        }

        internal static object getStatus()
        {
            if (statusBarLabel != null)
            {
                return statusBarLabel.Content;
            }
            return null;
        }

        internal static void clearStatus()
        {
            setStatus("");
        }

        public static bool _IsDesignTime = true;
        public static bool IsDesignTime { get { return _IsDesignTime; } }

        public static RoutedUICommand cmdAssetType = new RoutedUICommand("cmdAssetType", "cmdAssetType", typeof(Window1));
        public static RoutedUICommand cmdSearchType = new RoutedUICommand("cmdSearchType", "cmdSearchType", typeof(Window1));
        public static RoutedUICommand cmdCut = new RoutedUICommand("cmdCut", "cmdCut", typeof(Window1));
        public static RoutedUICommand cmdCopy = new RoutedUICommand("cmdCopy", "cmdCopy", typeof(Window1));
        public static RoutedUICommand cmdPaste = new RoutedUICommand("cmdPaste", "cmdPaste", typeof(Window1));
        public static RoutedUICommand cmdInsert = new RoutedUICommand("cmdInsert", "cmdInsert", typeof(Window1));
        public static RoutedUICommand cmdReport = new RoutedUICommand("cmdReport", "cmdReport", typeof(Window1));
        public static RoutedUICommand cmdService = new RoutedUICommand("cmdService", "cmdService", typeof(Window1));
        public static RoutedUICommand cmdCollapse = new RoutedUICommand("cmdCollapse", "cmdCollapse", typeof(Window1));
        public static RoutedUICommand cmdPriority = new RoutedUICommand("cmdPriority", "cmdPriority", typeof(Window1));
        public static RoutedUICommand cmdCategory = new RoutedUICommand("cmdCategory", "cmdCategory", typeof(Window1));
        public static RoutedUICommand cmdAutoSave = new RoutedUICommand("cmdAutoSave", "cmdAutoSave", typeof(Window1));
        public static RoutedUICommand cmdComments = new RoutedUICommand("cmdComments", "cmdComments", typeof(Window1));
        public static RoutedUICommand cmdDisableSpelling = new RoutedUICommand("cmdDisableSpelling", "cmdDisableSpelling", typeof(Window1));
        public static RoutedUICommand cmdValidation = new RoutedUICommand("cmdValidation", "cmdValidation", typeof(Window1));
        public static RoutedUICommand cmdHTMLValidation = new RoutedUICommand("cmdHTMLValidation", "cmdHTMLValidation", typeof(Window1));
        public static RoutedUICommand cmdAdditionalSettings = new RoutedUICommand("cmdAdditionalSettings", "cmdAdditionalSettings", typeof(Window1));
        public static RoutedUICommand cmdUpdateDervivedAssets = new RoutedUICommand("cmdUpdateDervivedAssets", "cmdUpdateDervivedAssets", typeof(Window1));
        public static RoutedUICommand cmdCheckTransfers = new RoutedUICommand("cmdCheckTransfers", "cmdCheckTransfers", typeof(Window1));
        public static RoutedUICommand cmdGetQuestionData = new RoutedUICommand("cmdGetQuestionData", "cmdGetQuestionData", typeof(Window1));
        public static RoutedUICommand cmdTriageMode = new RoutedUICommand("cmdTriageMode", "cmdTriageMode", typeof(Window1));
        public static RoutedUICommand cmdOtherData = new RoutedUICommand("cmdOtherData", "cmdOtherData", typeof(Window1));
        public static RoutedUICommand cmdUpdateApplication = new RoutedUICommand("cmdUpdateApplication", "cmdUpdateApplication", typeof(Window1));
        public static RoutedUICommand cmdUpdateGroups = new RoutedUICommand("cmdUpdateGroups", "cmdUpdateGroups", typeof(Window1));
        public static RoutedUICommand cmdAlgoManagement = new RoutedUICommand("cmdAlgoManagement", "cmdAlgoManagement", typeof(Window1));
        public static RoutedUICommand cmdTranslation = new RoutedUICommand("cmdTranslation", "cmdTranslation", typeof(Window1));
        public static RoutedUICommand cmdShowTranslation = new RoutedUICommand("cmdShowTranslation", "cmdShowTranslation", typeof(Window1));
        public static RoutedUICommand cmdAddLanguage = new RoutedUICommand("cmdAddLanguage", "cmdAddLanguage", typeof(Window1));
        public static RoutedUICommand cmdDeleteLanguage = new RoutedUICommand("cmdDeleteLanguage", "cmdDeleteLanguage", typeof(Window1));
        public static RoutedUICommand cmdEnableLanguageInheritance = new RoutedUICommand("cmdEnableLanguageInheritance", "cmdEnableLanguageInheritance", typeof(Window1));
        public static RoutedUICommand cmdAlgoTracker = new RoutedUICommand("cmdAlgoTracker", "cmdAlgoTracker", typeof(Window1));
        public static RoutedUICommand cmdRiskCalc = new RoutedUICommand("cmdRiskCalc", "cmdRiskCalc", typeof(Window1));
        public static RoutedUICommand cmdProperties = new RoutedUICommand("cmdProperties", "cmdProperties", typeof(Window1));
        public static RoutedUICommand cmdLaunchHelp = new RoutedUICommand("cmdLaunchHelp", "cmdLaunchHelp", typeof(Window1));
        public static RoutedUICommand cmdDebugApplication = new RoutedUICommand("cmdDebugApplication", "cmdDebugApplication", typeof(Window1));
        public static RoutedUICommand cmdCompareVersions = new RoutedUICommand("cmdCompareVersions", "cmdCompareVersions", typeof(Window1));
        public static RoutedUICommand cmdTableEdit = new RoutedUICommand("cmdTableEdit", "cmdTableEdit", typeof(Window1));
        public static RoutedUICommand cmdCodes = new RoutedUICommand("cmdCodes", "cmdCodes", typeof(Window1));
        public static RoutedUICommand cmdTraversalClient = new RoutedUICommand("cmdTraversalClient", "cmdTraversalClient", typeof(Window1));
        public static RoutedUICommand cmdTraversalFinder = new RoutedUICommand("cmdTraversalFinder", "cmdTraversalFinder", typeof(Window1));
        public static RoutedUICommand cmdUserManagement = new RoutedUICommand("cmdUserManagement", "cmdUserManagement", typeof(Window1));

        public Window1()
        {
            _IsDesignTime = DesignerProperties.GetIsInDesignMode(this);

            bool closeapp = false;
            while (!closeapp && !validateUri(Settings.Default.WebService))
            {
                closeapp = WebServiceFail();
            }
            if (closeapp)
            {
                Close();
                return;
            }
            DefaultLanguage = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
            InitializeComponent();
            if (System.Windows.SystemParameters.PrimaryScreenHeight < 1000) this.WindowState = WindowState.Maximized;

            statusBarLabel = statusBar;
            //cmbWebService.Items.Add(Properties.Settings.Default.WebService);
            qcat1.Form = this;
            window = this;
            textWrapping = FindResource("TextBlockWrapping") as Style;

            RecentAssets = recentAssets;
            try
            {
                if (!IsDesignTime)
                {
                    try
                    {
                        qcat1.Defaults = DataAccess.getDataNode("ab_builderdefaults", null, false);
                        XmlNode security = qcat1.Defaults.SelectSingleNode("//*/SecurityContext");
                        if (security != null)
                            Security = (SecurityContext) Enum.Parse(typeof (SecurityContext), security.InnerText);
                        string[] languages = Settings.Default.AvailableLanguages.Split(';');
                        foreach (var lang in languages)
                        {
                            if (lang == "") continue;
                            //RibbonGalleryItem item = new RibbonGalleryItem { Content = lang };
                            rtbLanguage.Items.Add(lang);
                        }
                        rtbLanguage.Loaded += (object sender, RoutedEventArgs e) =>
                        {
                            var ltb = (TextBox)rtbLanguage.Template.FindName("PART_EditableTextBox", rtbLanguage);
                            if (ltb != null)
                            {
                                ltb.SetValue(TextBox.MaxLengthProperty, 20);
                            }
                        };
                        rtbEnableLanguageInheritance.IsChecked = Settings.Default.HideTranslationInheritance;
                        EnableLanguageInheritance = Settings.Default.HideTranslationInheritance;
                        SetLanguageIheritanceButtonImage();
                    }
                    catch(System.Net.WebException)
                    {
                        Security = SecurityContext.Full;
                        if (WebServiceFail())
                        {
                            Close();
                            return;
                        }
                    }
                    catch
                    {
                        Security = SecurityContext.Full;
                    }
                    if (Security == SecurityContext.Full)
                    {
                        LoginWebService();
                    }
                    else LoggedIn = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            CollapseDisabled = true;
            if (Program.newVersionAvailable || true) btnUpdateApplication.IsEnabled = true;
            AssetBuilder.Controls.NLInfo nli = new AssetBuilder.Controls.NLInfo();
            NLI = nli;
            qcat1.InitDictionaries();
            qcat1.AssetTypeId = 2;
            UsernameValue.Text = Environment.UserName.Replace(".", "");
            //nli.Show();
        }

        private static bool WebServiceFail()
        {
            bool closeapp = false;
            InputBox ib =
                new InputBox(
                    "The default web serive URL does not exist or is invalid.\n\nPlease enter the correct web service address.",
                    "Incorrect Web Service Address", "URL", System.Windows.WindowStartupLocation.CenterScreen);
            ib.Text = Settings.Default.WebService;
            ib.ShowDialog();
            if (!ib.DialogResult.HasValue || !ib.DialogResult.Value) closeapp = true;
            else Settings.Default.WebService = ib.Text;
            return closeapp;
        }

        private void LoginWebService()
        {
            disableForm();
            UserName = null;
            Password = null;
            PreviousUserLevel = UserLevel;
            UserLevel = UserSecurityLevel.Unknown;
            Controls.linkControl lc = new AssetBuilder.Controls.linkControl();
            lc.WebServiceChanged += new SelectionChangedEventHandler(lc_WebServiceChanged);
            lc.setFormForLogin();
            qcat1.Visibility = Visibility.Collapsed;
            Point p = new Point(
                (this.Width - lc.Width) / 2,
                100);
            lc.SetValue(Canvas.LeftProperty, p.X);
            lc.SetValue(Canvas.TopProperty, p.Y);
            lc.btnCancel.Click += new RoutedEventHandler(CloseApp);
            lc.btnOK.Click += new RoutedEventHandler(delegate (object obj, RoutedEventArgs ev)
            {
                lc_WebServiceChanged(lc, null);
                if (Security != SecurityContext.Full && lc.txtDisplay.Text == "" && lc.txtPassword.Password == "")
                {
                    qcat1.Repopulate(1);
                    btnCancel_Click(null, null);
                    qcat1.ClearCats();
                    LoggedIn = true;
                    saveSettings();
                    SetFeatures();
                    return;
                }
                UserName = lc.txtDisplay.Text;
                Password = lc.txtPassword.Password;
                if (UserName == "")
                {
                    MessageBox.Show("Please provide a Username", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    LoggedIn = false;
                    return;
                }
                if (Password == "")
                {
                    MessageBox.Show("Please provide a Password", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    LoggedIn = false;
                    return;
                }
                XmlNode testUser = DataAccess.getDataNode("ab_builderdefaults", null, true);
                if (testUser.Name == "Error")
                {
                    MessageBox.Show("Login unsuccessful", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    LoggedIn = false;
                    return;
                }
                qcat1.Visibility = Visibility.Visible;
                qcat1.Defaults = testUser;
                XmlNode userlevel = qcat1.Defaults.SelectSingleNode("//*/UserLevel");
                if (userlevel != null) UserLevel = (UserSecurityLevel)Enum.Parse(typeof(UserSecurityLevel), userlevel.InnerText);
                else UserLevel = UserSecurityLevel.Unknown;
                saveSettings();
                //qcat1.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
                //qcat1.SetAdorner(null, null);
                qcat1.Repopulate(1);
                qcat1.ClearCats();
                LoggedIn = true;
                btnCancel_Click(obj, ev);
                SetFeatures();
            });
            bubbleCanvas.Children.Add(lc);
            FocusManager.SetFocusedElement(this, lc.txtDisplay);
        }

        private void SetFeatures()
        {
            string[] features = qcat1.Defaults.SelectNodes("//*/Feature").OfType<XmlNode>().Select(f => f.InnerText).ToArray();
            AllowConclusionMap = /*UserName.ToLower() == "e24"; //*/ features.Contains("ConclusionMap");
            AssetMapping = features.Contains("AssetMapping");
            AllowProperties = features.Contains("Properties");
            AllowTextAssets = features.Contains("TextAssets");
            AllowAudit = features.Contains("Audit");
            if (AllowTextAssets)
            {
                var xml = DataAccess.getData("gettextassetlocation");
                AssetBuilder.Properties.Settings.Default.TextAssetLocation = xml?.Element("Table")?.Value;
            }
            AllowGroups = features.Contains("Groups");
            AllowRiskCalculator = features.Contains("RiskCalculator");
            AllowSaaSIntegration = features.Contains("SaaSIntegration");
            AssetListIcons = features.Contains("AssetListIcons");
            AllowExportReport = features.Contains("ExportReport");
            AllowReleaseStatusView = features.Contains("ReleaseStatusView");
            if(AssetListIcons)
            {
                qcat.AssetFlags = new Dictionary<int, Dictionary<int, int>>
                {
                    { 1, new Dictionary<int, int>() },
                    { 2, new Dictionary<int, int>() },
                    { 3, new Dictionary<int, int>() },
                    { 4, new Dictionary<int, int>() },
                    { 5, new Dictionary<int, int>() }
                };
                var sources = new[] { 1, 2 }; // Mother Algos - Dynamic - provided by user
                foreach (var item in sources)
                {
                    var source = new Uri(new Uri(Settings.Default.WebService), $"TraversalService/TableOutput/AssetBuilderFunction_AssetsFromMotherAlgo/json/array/{item}").AbsoluteUri;
                    var jnode = source.GetContent<JNode>();
                    foreach (var j in jnode)
                    {
                        var type = j["TypeID"];
                        var id = j["AssetID"];
                        if (!qcat.AssetFlags[type].ContainsKey(id)) qcat.AssetFlags[type].Add(id, 0);
                        qcat.AssetFlags[type][id] |= item;
                    }
                }
            }
            else
            {
                qcat.AssetFlags = null;
            }
            var settings = qcat1.Defaults.SelectNodes("//*[Key]").OfType<XmlNode>().Select(f => (key: f["Key"].InnerText, value: f["Value"].InnerText)).ToArray();
            foreach (var setting in settings)
            {
                var key = setting.key.Replace("!", "");
                var value = setting.key.StartsWith("!") ? setting.value.Decrypt() : setting.value;
                if (Properties.Settings.Default[key] != null) Properties.Settings.Default[key] = value;
            }
            AllowTableEdit = features.Contains("TableEdit");
            AllowGraph = features.Contains("Graph");
            if (!AllowGraph)
            {
                XmlNodeList xnl = qcat1.Defaults.SelectNodes("*[Exclude='graphButton']");
                foreach (XmlNode item in xnl)
                {
                    item.InnerText += "_disabled";
                }
            }
            RiskMotherAlgo = int.Parse(qcat1?.Defaults?.SelectSingleNode("*/RiskMotherAlgo")?.InnerText ?? "2530");

            btnProperties.Visibility = AllowProperties ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnLinkAssets.Visibility = AssetMapping ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnGroup.Visibility = AllowGroups ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnUpdateGroups.Visibility = AllowGroups ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            rtbText.Visibility = AllowTextAssets ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnRiskCalculator.Visibility = AllowRiskCalculator ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnTableEdit.Visibility = AllowTableEdit ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            SetOtherDataReports();

            qcat1.InitDictionaries();
        }

        private void SetOtherDataReports()
        {
            var reports = qcat1.Defaults.SelectNodes("//*[Title and Procedure and ReportName]").OfType<XmlNode>().Select(f => new OtherReport
            {
                Title = f["Title"].InnerText,
                Procedure = f["Procedure"].InnerText,
                ReportName = f["ReportName"].InnerText,
                Parameters = f["Parameters"]?.InnerText
            });

            OtherDataMenu.Items.Clear();
            OtherDataMenu.Visibility = reports.Any() ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in reports)
            {
                OtherDataMenu.Items.Add(new RibbonMenuItem
                {
                    Header = item.Title,
                    ToolTip = new Info { Image = "/images/AssetReport.png", Body = "OtherDataReportToolTip", Title = item.Title }.ProvideValue(null),
                    ImageSource = new BitmapImage(new Uri(@"/images/AssetReport.png", UriKind.Relative)),
                    Command = cmdOtherData,
                    CommandParameter = item
                });
            }
        }

        bool validateUri(string uri)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)HttpWebRequest.Create(uri);
                wr.Method = "GET";
                using (HttpWebResponse resp = (HttpWebResponse) wr.GetResponse())
                {
                    string type = resp.Headers["Content-type"];
                    var status = resp.StatusCode;
                    resp.Close();
                    if (type.StartsWith("text/") && status == HttpStatusCode.OK) return true;
                }
            }
            catch
            {
            }
            return false;
        }

        bool suspend = false;

        void lc_WebServiceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suspend) return;
            Controls.linkControl lc = sender as Controls.linkControl;
            //if (Settings.Default.WebService == lc.cmbWebService.Text) return;
            string webservice = lc.cmbWebService.Text;
            if (e != null && e.AddedItems != null && e.AddedItems.Count > 0) webservice = (string)e.AddedItems[0];
            //if (Settings.Default.WebService == webservice || webservice == "") return;
            bool isvalid = true;// validateUri(webservice);
            if (!isvalid)
            {
                lc.dontsave.Add(webservice);
                if (lc.cmbWebService.Items.Contains(webservice)) lc.cmbWebService.Items.Remove(webservice);
                return;
            }
            string saveValue = Settings.Default.WebService;
            Settings.Default.WebService = webservice;
            var mm = webservice.IndexOf("mckesson", StringComparison.CurrentCultureIgnoreCase) > -1 || webservice.IndexOf("aph", StringComparison.CurrentCultureIgnoreCase) > -1;
            //McKesson_Mode = webservice.IndexOf("mckesson", StringComparison.CurrentCultureIgnoreCase) > -1 || webservice.IndexOf("aph", StringComparison.CurrentCultureIgnoreCase) > -1;
            if (mm)
                hdrMcKesson.Visibility = System.Windows.Visibility.Visible;
            else
                hdrMcKesson.Visibility = System.Windows.Visibility.Collapsed;

            try
            {
                UserName = "";
                Password = "";
                if (webservice != "" && !lc.dontsave.Contains(webservice))
                    qcat1.Defaults = DataAccess.getDataNode("ab_builderdefaults", null, false);
            }
            catch (Exception ex)
            {
                MessageBoxResult mbr = MessageBox.Show("An invalid URL was entered.\n\n" + ex.Message + "\n\nDo you want it removed from the list?", "Cannot connect to Web Service", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                suspend = true;
                if (mbr == MessageBoxResult.Yes) lc.cmbWebService.Text = "";
                if (mbr == MessageBoxResult.Yes && lc.cmbWebService.Items.Contains(webservice)) lc.cmbWebService.Items.Remove(webservice);
                suspend = false;
                lc.dontsave.Add(webservice);
                Settings.Default.WebService = saveValue;
                throw ex;
            }
            XmlNode security = qcat1.Defaults.SelectSingleNode("//*/SecurityContext");
            if (security != null) Security = (SecurityContext)Enum.Parse(typeof(SecurityContext), security.InnerText);
            else Security = SecurityContext.Open;
            if (Security == SecurityContext.Open)
            {
                lc.txtDisplay.IsEnabled = false;
                lc.txtPassword.IsEnabled = false;
                lc.txtDisplay.Text = "";
                lc.txtPassword.Password = "";
            }
            else
            {
                lc.txtDisplay.IsEnabled = true;
                lc.txtPassword.IsEnabled = true;
            }
            if (e != null) Settings.Default.WebService = saveValue;
            if (!lc.dontsave.Contains(saveValue)) addSettingsValue("PreviousWebService", saveValue);
        }

        public static void addSettingsValue(string setting, string saveValue)
        {
            string value = Settings.Default[setting].ToString();
            List<string> previous = new List<string>(value.Split(';'));
            if (!previous.Contains(saveValue))
            {
                string newPrev = value;
                if (newPrev != "") newPrev += ";";
                newPrev += saveValue;
                Settings.Default[setting] = newPrev;
                saveSettings();
            }
        }

        public static void updateSettingsValue(string setting, ItemCollection items)
        {
            string value = "";
            foreach (var item in items)
            {
                if (value != "") value += ";";
                if (item is RibbonGalleryItem)
                    value += (item as RibbonGalleryItem).Content.ToString();
                else
                    value += item.ToString();
            }
            Settings.Default[setting] = value;
            Settings.Default.Save();
        }

        private static void addPreviousWebService(string saveValue)
        {
            List<string> previousWS = new List<string>(Settings.Default.PreviousWebService.Split(';'));
            if (!previousWS.Contains(saveValue))
            {
                string newPrev = Settings.Default.PreviousWebService;
                if (newPrev != "") newPrev += ";";
                newPrev += saveValue;
                Settings.Default.PreviousWebService = newPrev;
            }
        }

        void CloseApp(object sender, RoutedEventArgs e)
        {
            if (LoggedIn) btnCancel_Click(null, null);
            else this.Close();
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void exitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            NLI?.Close();
            _userManagementWindow?.Close();
        }

        qcat qcat2 = null;

        public void clearSubCat()
        {
            if (qcat2 != null)
            {
                mainGrid.Children.Remove(qcat2);
                qcat2 = null;
                qcat1.Visibility = Visibility.Visible;
                qcat1.SetAdorners();
            }
        }

        private void assetType_Click(object sender, ExecutedRoutedEventArgs e)
        {
            changeAssetType(e.Parameter);
        }

        public void changeAssetType(object e)
        {
            RibbonGroup rg = assetGroup;
            RadioToggle(rg, e);
            clearSubCat();
            if (e != null)
            {
                int asset = int.Parse(e.ToString());
                if (qcat1.AssetTypeId == 4 && e.ToString() == "5" && qcat1.IsEditing && (qcat1.LoadedAsset as AssetControls.Conclusion).bulletsVisible)
                {
                    qcat1.Visibility = Visibility.Collapsed;
                    qcat1.ClearAdorners();
                    qcat2 = new qcat { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 3), AssetTypeId = 5 };
                    qcat2.SearchTypeId = qcat1.SearchTypeId;
                    qcat2.SetButtons(false);
                    qcat2.LoadedAsset = qcat1.LoadedAsset;
                    mainGrid.Children.Add(qcat2);
                }
                else if (qcat1.AssetTypeId != asset)
                {
                    rtbAutoSave.IsChecked = false;
                    Change_AutoSave(null, null);
                    qcat1.AssetTypeId = asset;
                }
            }
            HideBrowsers();
        }

        public void HideBrowsers(string type = null)
        {
            if (type == null || type != "TraversalClient")
            {
                if (TraversalClient != null) FullPanel.Children.Remove(TraversalClient);
                rtbTraversalClient.IsChecked = false;
            }
            if (type == null || type != "TraversalFinder")
            {
                if (TraversalFinder != null) FullPanel.Children.Remove(TraversalFinder);
                rtbTraversalFinder.IsChecked = false;
            }
        }

        private void searchType_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == "-1")
            {
                SearchTranslation = (bool)rtbSearchTranslation.IsChecked;
                if (SearchTranslation)
                {
                    rtbSearchTranslation.SmallImageSource = new BitmapImage(new Uri("images/EnableLanguage16x16.png", UriKind.Relative));
                    rtbSearchTranslation.LargeImageSource = new BitmapImage(new Uri("images/EnableLanguage32x32.png", UriKind.Relative));
                }
                else
                {
                    rtbSearchTranslation.SmallImageSource = new BitmapImage(new Uri("images/DisableLanguage16x16.png", UriKind.Relative));
                    rtbSearchTranslation.LargeImageSource = new BitmapImage(new Uri("images/DisableLanguage32x32.png", UriKind.Relative));
                }
                qcat1.SearchTypeId = qcat1.SearchTypeId;
            }
            else
            {
                RibbonGroup rg = searchGroup;
                RadioToggle(rg, e.Parameter);
                if (e.Parameter != null)
                {
                    int asset = int.Parse(e.Parameter.ToString());
                    qcat1.SearchTypeId = asset;
                    if (qcat2 != null) qcat2.SearchTypeId = asset;
                }
            }
        }

        private void insert_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canInsert;
        }

        private void insert_Click(object sender, ExecutedRoutedEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case "1":
                    insertHyperLink();
                    break;
                case "2":
                    insertYouTube();
                    break;
                case "3":
                    insertPicture();
                    break;
                default:
                    break;
            }
        }

        private void report_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = qcat2 == null && qcat1 != null && qcat1.listBox5.Items.Count > 0; // TODO: To ViewModel
        }

        private void report_Click(object sender, ExecutedRoutedEventArgs e)
        {
            XmlDocument doc = qcat1.GetAssetXml("test", true);
            if (Window1.ShowTranslation)
                Reports.Report.RunReport("AssetReport", new List<string>(new string[] { "AssetReport", "assetxml", doc.OuterXml, "currentDate", "addedColumns:" + qcat1.AssetTypeId, "merge:" }));
            else
                Reports.Report.RunReport("AssetReport", new List<string>(new string[] { "AssetReport", "assetxml", doc.OuterXml, "currentDate", "addedColumns:" + qcat1.AssetTypeId }));
        }

        private void insertPicture()
        {
            string link = @"<img src=""{1}"" alt=""{0}"" />";
            addLink("Alt Text:", "Source:", link, false, true);
        }

        private void insertHyperLink()
        {
            string link = @"<a href=""{1}"" target=""_blank"">{0}</a>";
            addLink("Text to Display:", "Address:", link, true, true);
        }

        private void insertYouTube()
        {
            string link = @"<object width=""425"" height=""344""><param name=""movie"" value=""http://www.youtube.com/v/"
                  + "{0}" + @"&hl=en&fs=1&rel=0&autoplay=1""></param><param name=""allowFullScreen"" value=""true""></param><param name=""allowscriptaccess"" value=""always""></param><embed src=""http://www.youtube.com/v/"
                  + "{0}" + @"&hl=en&fs=1&rel=0&autoplay=1"" type=""application/x-shockwave-flash"" allowscriptaccess=""always"" allowfullscreen=""true"" width=""425"" height=""344""></embed></object>";
            addLink("Reference:", "", link, true, false);
        }

        private void addLink(string lbl1, string lbl2, string link, bool txt1Required, bool txt2Required)
        {
            if (assetCanvas.Children.Count > 0 && qcat1.IsEditing)
            {
                System.Windows.IInputElement ie = FocusManager.GetFocusedElement(this);
                if (ie is TextBox)
                {
                    TextBox t = ie as TextBox;
                    int ss = t.SelectionStart; int se = t.SelectionLength + ss;
                    string s = t.Text;

                    disableForm();
                    Controls.linkControl lc = new AssetBuilder.Controls.linkControl();
                    if (lbl1 == "") lc.txtDisplay.Visibility = Visibility.Collapsed;
                    if (lbl2 == "") lc.txtAddress.Visibility = Visibility.Collapsed;
                    lc.lblDisplay.Content = lbl1;
                    lc.lblAddress.Content = lbl2;
                    if (lbl1 == "Alt Text:")
                        lc.lblDisplay.ToolTip = new AssetBuilder.Info { Title = "Alt Text", Body = "AltTextToolTip" }.ProvideValue(null);
                    else
                        lc.lblDisplay.ToolTip = null;
                    lc.lblAddress.ToolTip = null;
                    lc.txtDisplay.ToolTip = null;
                    lc.txtAddress.ToolTip = null;
                    Point p = new Point(
                        (qcat1.ActualWidth - lc.Width) / 2,
                        (qcat1.ActualHeight - lc.Height) / 2);
                    lc.SetValue(Canvas.LeftProperty, p.X);
                    lc.SetValue(Canvas.TopProperty, p.Y);
                    lc.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
                    lc.btnOK.Click += new RoutedEventHandler(delegate (object obj, RoutedEventArgs ev)
                    {
                        if (txt1Required && lc.txtDisplay.Text == "")
                        {
                            MessageBox.Show("Please provide a " + lbl1, "Cannot create link", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (txt2Required && lc.txtAddress.Text == "")
                        {
                            MessageBox.Show("Please provide a " + lbl2, "Cannot create link", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        link = string.Format(link, lc.txtDisplay.Text, lc.txtAddress.Text);
                        t.Text = s.Substring(0, ss) + link + s.Substring(se);
                        t.SelectionStart = ss + link.Length;
                        t.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                        btnCancel_Click(obj, ev);
                    });
                    bubbleCanvas.Children.Add(lc);
                    FocusManager.SetFocusedElement(this, lc.txtDisplay);
                }
            }
        }

        public void setConclusionTitle(XmlDocument xml)
        {
            XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                "@xml", xml.OuterXml
            }, false);
            if (xn.Element("Table") != null && xn.Element("Table").Element("Title") != null)
            {
                disableForm();
                AssetBuilder.Controls.linkControl lc = new linkControl();
                bool symptom = xml.DocumentElement?.GetAttribute("CategoryTypeID") == "1";
                lc.setFormForTitle((qcat1.ActualWidth - lc.Width) / 2, (qcat1.ActualHeight - lc.Height) / 2, xn.Element("Table").Element("Title").Value, symptom);
                lc.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
                int rt;
                if (xn.Element("Table").Element("RecTypeID") != null && int.TryParse(xn.Element("Table").Element("RecTypeID").Value, out rt))
                {
                    lc.btnProperties.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs ev)
                    {
                        Controls.Properties.CreateProperties(AssetType.Title, rt.ToString()).Show();
                    });
                }
                else lc.btnProperties.Visibility = Visibility.Collapsed;
                lc.btnOK.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs ev)
                {
                    if (Window1.EditTranslation)
                    {
                        if(symptom && lc.txtTranslation.Text.Split('|').Where(f => !string.IsNullOrWhiteSpace(f)).Count() != 3)
                        {
                            MessageBox.Show("Cannot save title this would be an invalid symptom conclusion title.", "Invalid Title", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                        if (!NLExtensions.validateTextBox(lc.txtTranslation))
                        {
                            MessageBox.Show("Cannot save title because of natural language errors.", "Natural Language Errors", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                        xml["root"].AppendChild(xml.CreateElement("Title")).InnerText = lc.txtTranslation.Text;
                        DataAccess.setLanguage(0, lc.txtAddress.Text, xml, Window1.TranslationLanguage);
                    }
                    else
                    {
                        if (symptom && lc.txtAddress.Text.Split('|').Where(f => !string.IsNullOrWhiteSpace(f)).Count() != 3)
                        {
                            MessageBox.Show("Cannot save title this would be an invalid symptom conclusion title.", "Invalid Title", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                        if (!NLExtensions.validateTextBox(lc.txtAddress))
                        {
                            MessageBox.Show("Cannot save title because of natural language errors.", "Natural Language Errors", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                        xml["root"].AppendChild(xml.CreateElement("Title")).InnerText = lc.txtAddress.Text;
                        DataAccess.getData("ab_UpdateAsset", new string[] {
                            "@xml", xml.OuterXml
                        }, true);
                    }
                    btnCancel_Click(null, null);
                });
                bubbleCanvas.Children.Add(lc);
                FocusManager.SetFocusedElement(this, lc.txtAddress);
            }
        }

        public void disableForm()
        {
            greyCanvas.Visibility = Visibility.Visible;
            rbnApplication.IsEnabled = false;
            mainGrid.IsEnabled = false;
            gridBlur.Radius = 5d;
            qcat1.ClearAdorners();
            greyCanvas.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public void enableForm()
        {
            greyCanvas.Visibility = Visibility.Hidden;
            rbnApplication.IsEnabled = true;
            mainGrid.IsEnabled = true;
            gridBlur.Radius = 0d;
            qcat1.SetAdorners();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) UserLevel = PreviousUserLevel;
            qcat1.Visibility = Visibility.Visible;
            bubbleCanvas.Children.Clear();
            enableForm();
        }

        public static void RadioToggle(RibbonGroup rg, object parameter)
        {
            foreach (var item in rg.Items)
            {
                if (item is RibbonToggleButton)
                {
                    RibbonToggleButton tb = item as RibbonToggleButton;
                    if (tb.CommandParameter.ToString() == parameter.ToString())
                        tb.IsChecked = true;
                    else
                        tb.IsChecked = false;
                }
            }
        }

        private void Undo_Click(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Undo.Execute(null, null);
        }

        private void Redo_Click(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Redo.Execute(null, null);
        }

        private void Collapse_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbCollapse.IsChecked)
            {
                rtbCollapse.SmallImageSource = new BitmapImage(new Uri("images/ShowDisabled_16x16.png", UriKind.Relative));
                rtbCollapse.LargeImageSource = new BitmapImage(new Uri("images/ShowDisabled_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbCollapse.SmallImageSource = new BitmapImage(new Uri("images/NoShowDisabled_16x16.png", UriKind.Relative));
                rtbCollapse.LargeImageSource = new BitmapImage(new Uri("images/NoShowDisabled_32x32.png", UriKind.Relative));
            }
            CollapseDisabled = !(bool)rtbCollapse.IsChecked;
        }

        private void Update_DerivedAssets(object sender, ExecutedRoutedEventArgs e)
        {
            XmlDocument doc;
            XmlElement root;
            qcat.CreateUpdateXml("updateDerived", out doc, out root);
            AssetControls.assetControl.RunUpdate(doc);
        }

        private void Get_Question_Data(object sender, ExecutedRoutedEventArgs e)
        {
            var data = DataAccess.getData("abmk_GetQuestionData", "@QuestionID", qcat1.AssetId);
            var items = data.Elements().Select(f => new
            {
                AlgoName = f.Element("Algo_Name").Value,
                Question = f.Element("Question").Value,
                Yes = f.Element("Yes").Value,
                No = f.Element("No").Value,
            });

            displaygridwindow(items, "Question " + qcat1.AssetId + " Data");
        }

        private void Get_Other_Data(object sender, ExecutedRoutedEventArgs e)
        {
            var otherreport = (OtherReport)e.Parameter;
            string report = otherreport.ReportName;
            XmlDocument xml = qcat1.GetAssetXml(report, qcat1.TableNames[qcat1.AssetTypeId]);
            xml.DocumentElement.Attributes.Append(xml.CreateAttribute("search")).Value = qcat.CurrentSearchSql;
            var data = DataAccess.getData(otherreport.Procedure, "@xml", xml.OuterXml);
            var items = new DataTable();
            var xElement = data.Element("Table");
            if (xElement == null) return;
            foreach (var item in xElement.Elements())
            {
                items.Columns.Add(item.Name.ToString().Replace("_x0020_", " "));
            }
            foreach (var item in data.Elements())
            {
                items.Rows.Add(item.Elements().Select(f => (object)f.Value).ToArray());
                //var dynamic = new ExpandoObject() as IDictionary<string, object>;
                //foreach (var col in item.Elements())
                //{
                //    dynamic.Add(col.Name.ToString(), col.Value);
                //}
                //items.Add(dynamic as dynamic);
            }
            //var items = data.Elements().Select(f => new
            //{
            //    AlgoID= f.Element("AlgoID").Value,
            //    Algo_Name = f.Element("Algo_Name").Value,
            //    Module = f.Element("Module").Value,
            //    Category = f.Element("Category").Value,
            //});
            displaygridwindow(items.DefaultView, report);
        }

        static Style textWrapping; 

        public static void displaygridwindow(DataView data, string title)
        {
            //ObservableCollection<T> data = new ObservableCollection<T>(items);
            var window = new ABWindow { Title = title, ResizeMode = System.Windows.ResizeMode.NoResize };
            var grid = new DataGrid() { MaxColumnWidth = 400, AutoGenerateColumns = true, CanUserAddRows = false, CanUserDeleteRows = false, IsReadOnly = true };
            grid.AutoGeneratingColumn += (sender, args) =>
            {
                if (args.Column is DataGridTextColumn)
                {
                    (args.Column as DataGridTextColumn).ElementStyle = Window1.textWrapping;// .Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                }
                if(args.PropertyName.StartsWith("x_")) args.Column.Visibility = Visibility.Collapsed;
            };
            window.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            window.MaxHeight = 500;
            window.MaxWidth = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Window1.window.Left, (int)Window1.window.Top)).WorkingArea.Width - 100;
            grid.ItemsSource = data;
            grid.ClipboardCopyMode = DataGridClipboardCopyMode.None;
            grid.KeyUp += Grid_KeyUp;
            window.Content = grid;
            window.ShowDialog();
        }

        private static void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            var clip = "";
            if (sender is DataGrid dg && Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                foreach (var item in dg.Columns)
                {
                    if (clip != "") clip += "\t";
                    clip += "\"" + item.Header.ToString().Replace("\"", "\"\"") + "\"";
                }
                foreach (DataRowView item in dg.SelectedItems)
                {
                    var row = "";
                    foreach (var field in item.Row.ItemArray)
                    {
                        if (row != "") row += "\t";
                        row += "\"" + field.ToString().Replace("\"", "\"\"") + "\"";
                    }
                    clip += Environment.NewLine + row;
                }
                Clipboard.SetText(clip);
            }
        }

        public static void displaygridwindow<T>(IEnumerable<T> items, string title)
        {
            ObservableCollection<T> data = new ObservableCollection<T>(items);
            var window = new ABWindow { Title = title, ResizeMode = System.Windows.ResizeMode.NoResize };
            var grid = new DataGrid() { AutoGenerateColumns = true, CanUserAddRows = false, CanUserDeleteRows = false, IsReadOnly = true };
            window.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            window.MaxHeight = 500;
            grid.ItemsSource = data;
            window.Content = grid;
            window.ShowDialog();
        }

        private void Check_Transfers(object sender, ExecutedRoutedEventArgs e)
        {
            var badtransfers = DataAccess.getData("abmk_CheckTransfers");
            var items = badtransfers.Elements().Select(f => new
            {
                FromAlgoID = f.Element("FromAlgoID").Value,
                ToAlgoID = f.Element("ToAlgoID").Value,
                FromAlgo = f.Element("FromAlgo").Value,
                ToAlgo = f.Element("ToAlgo").Value,
                FromNodeID = f.Element("FromNodeID").Value,
                ToNodeID = f.Element("ToNodeID").Value,
                VisioShape = f.Element("VisioShape").Value,
            });
            string message = string.Join("\r\n", items.Select(f => string.Format("{3}|{5}|{6} - Missing Node {2} in Algo {0} {1} From Algo {3} {4} Node {5}", f.ToAlgoID, f.ToAlgo, f.ToNodeID, f.FromAlgoID, f.FromAlgo, f.FromNodeID, f.VisioShape)));
            Diva.Controls.Simple.CustomMessageBox.Show(message, "Broken Transfers", new string[] { "OK" }, "OK", "OK", true);
        }

        private void Change_AutoSave(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbAutoSave.IsChecked)
            {
                MessageBoxResult mbr = MessageBox.Show("Enabling autosave can be a dangerous thing to do, and should be done only by experienced users. For any given asset type it will put the system into edit mode and any changes you make to an asset will automatically be saved when you move off that asset unless you click \"Cancel\". This allows you to make a series of changes to multiple assets without clicking on Edit and Save after each change. You will not be prompted for the reason for the changes.", "Warning!!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (mbr == MessageBoxResult.Cancel)
                {
                    rtbAutoSave.IsChecked = false;
                    return;
                }
            }
            if ((bool)rtbAutoSave.IsChecked)
            {
                rtbAutoSave.SmallImageSource = new BitmapImage(new Uri("images/AutoSave_16x16.png", UriKind.Relative));
                rtbAutoSave.LargeImageSource = new BitmapImage(new Uri("images/AutoSave_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbAutoSave.SmallImageSource = new BitmapImage(new Uri("images/NoAutoSave_16x16.png", UriKind.Relative));
                rtbAutoSave.LargeImageSource = new BitmapImage(new Uri("images/NoAutoSave_32x32.png", UriKind.Relative));
            }
            AutoSave = (bool)rtbAutoSave.IsChecked;
        }

        private void Change_Comments(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbComments.IsChecked)
            {
                rtbComments.SmallImageSource = new BitmapImage(new Uri("images/Comment_16x16.png", UriKind.Relative));
                rtbComments.LargeImageSource = new BitmapImage(new Uri("images/Comment_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbComments.SmallImageSource = new BitmapImage(new Uri("images/DisableComment_16x16.png", UriKind.Relative));
                rtbComments.LargeImageSource = new BitmapImage(new Uri("images/DisableComment_32x32.png", UriKind.Relative));
            }
            DisableComments = !(bool)rtbComments.IsChecked;
        }

        private void Change_DisableSpelling(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbDisableSpelling.IsChecked)
            {
                rtbDisableSpelling.SmallImageSource = new BitmapImage(new Uri("images/DisableSpelling_16x16.png", UriKind.Relative));
                rtbDisableSpelling.LargeImageSource = new BitmapImage(new Uri("images/DisableSpelling_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbDisableSpelling.SmallImageSource = new BitmapImage(new Uri("images/NoDisableSpelling_16x16.png", UriKind.Relative));
                rtbDisableSpelling.LargeImageSource = new BitmapImage(new Uri("images/NoDisableSpelling_32x32.png", UriKind.Relative));
            }
            DisableSpelling = !(bool)rtbDisableSpelling.IsChecked;
            if (qcat1.LoadedAsset != null)
                foreach (TextBox item in qcat1.LoadedAsset.SpellChildren)
                    item.SpellCheck.IsEnabled = !DisableSpelling;
        }

        private void Change_Validation(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbValidation.IsChecked)
            {
                rtbValidation.SmallImageSource = new BitmapImage(new Uri("images/Validation_16x16.png", UriKind.Relative));
                rtbValidation.LargeImageSource = new BitmapImage(new Uri("images/Validation_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbValidation.SmallImageSource = new BitmapImage(new Uri("images/NoValidation_16x16.png", UriKind.Relative));
                rtbValidation.LargeImageSource = new BitmapImage(new Uri("images/NoValidation_32x32.png", UriKind.Relative));
            }
            DisableValidation = !(bool)rtbValidation.IsChecked;
            if ((bool)rtbHTML.IsChecked)
            {
                rtbHTML.IsChecked = false;
                Change_HTMLValidation(null, null);
            }
            else
            {
                if (qcat1.LoadedAsset != null)
                    foreach (TextBox item in qcat1.LoadedAsset.SpellChildren)
                        item.validateTextBox();
            }
        }

        private void Change_HTMLValidation(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbHTML.IsChecked)
            {
                rtbHTML.SmallImageSource = new BitmapImage(new Uri("images/EnableHTML_32x32.png", UriKind.Relative));
                rtbHTML.LargeImageSource = new BitmapImage(new Uri("images/EnableHTML_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbHTML.SmallImageSource = new BitmapImage(new Uri("images/DisableHTML_32x32.png", UriKind.Relative));
                rtbHTML.LargeImageSource = new BitmapImage(new Uri("images/DisableHTML_32x32.png", UriKind.Relative));
            }
            DisableHTMLValidation = !(bool)rtbHTML.IsChecked;
            if (qcat1.LoadedAsset != null)
                foreach (TextBox item in qcat1.LoadedAsset.SpellChildren)
                    item.validateTextBox();
        }

        private void Change_TriageMode(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbTriageMode.IsChecked)
            {
                rtbTriageMode.SmallImageSource = new BitmapImage(new Uri("images/EnableNurse_32x32.png", UriKind.Relative));
                rtbTriageMode.LargeImageSource = new BitmapImage(new Uri("images/EnableNurse_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbTriageMode.SmallImageSource = new BitmapImage(new Uri("images/DisableNurse_32x32.png", UriKind.Relative));
                rtbTriageMode.LargeImageSource = new BitmapImage(new Uri("images/DisableNurse_32x32.png", UriKind.Relative));
            }
            McKesson_Mode = (bool)rtbTriageMode.IsChecked;
        }

        private void AdditionalSettings(object sender, ExecutedRoutedEventArgs e)
        {
            disableForm();
            var add = new Dictionary<string, Control>();
            AssetBuilder.Controls.linkControl lc = new linkControl();
            Point p = new Point(
                (this.Width - lc.Width) / 2,
                100);
            lc.SetValue(Canvas.LeftProperty, p.X);
            lc.SetValue(Canvas.TopProperty, p.Y);
            if (Controls.Properties.CustomProperties.Any())
            {
                lc.btnProperties.Visibility = Visibility.Visible;
            }
            lc.lblAddress.Content = "Text Asset Location";
            lc.lblDisplay.Content = "Encyclopaedia Link";
            lc.txtAddress.Text = Settings.Default.TextAssetLocation;
            lc.txtDisplay.Text = Settings.Default.EncyclopaediaLink;
            if(AllowSaaSIntegration)
            {
                lc.Height += 4 * 29;
                add.Add("SaaSIdentity", new TextBox { IsEnabled = false, Text = Settings.Default.SaaSIdentity, Height = 23, Margin = new Thickness(138, 0, 12, 94), VerticalAlignment = VerticalAlignment.Bottom });
                add.Add("SaaSEndpoint", new TextBox { IsEnabled = false, Text = Settings.Default.SaaSEndpoint, Height = 23, Margin = new Thickness(138, 0, 12, 65), VerticalAlignment = VerticalAlignment.Bottom });
                add.Add("ClientID", new TextBox { IsEnabled = false, Text = Settings.Default.ClientID, Height = 23, Margin = new Thickness(138, 0, 12, 36), VerticalAlignment = VerticalAlignment.Bottom });
                add.Add("Secret", new PasswordBox { IsEnabled = false, Password = Settings.Default.Secret, Height = 23, Margin = new Thickness(138, 0, 12, 7), VerticalAlignment = VerticalAlignment.Bottom });
                lc.AdditionalFields.Children.Add(add["SaaSIdentity"]);
                lc.AdditionalFields.Children.Add(add["SaaSEndpoint"]);
                lc.AdditionalFields.Children.Add(add["ClientID"]);
                lc.AdditionalFields.Children.Add(add["Secret"]);
                lc.AdditionalFields.Children.Add(new Label { Height = 28, Margin = new Thickness(12, 0, 0, 89), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, HorizontalContentAlignment = HorizontalAlignment.Right, Width = 120, Content = "SaaS Identity" });
                lc.AdditionalFields.Children.Add(new Label { Height = 28, Margin = new Thickness(12, 0, 0, 60), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, HorizontalContentAlignment = HorizontalAlignment.Right, Width = 120, Content = "SaaS Endpoint" });
                lc.AdditionalFields.Children.Add(new Label { Height = 28, Margin = new Thickness(12, 0, 0, 31), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, HorizontalContentAlignment = HorizontalAlignment.Right, Width = 120, Content = "ClientID" });
                lc.AdditionalFields.Children.Add(new Label { Height = 28, Margin = new Thickness(12, 0, 0, 2), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, HorizontalContentAlignment = HorizontalAlignment.Right, Width = 120, Content = "Secret" });
            }
            bubbleCanvas.Children.Add(lc);
            lc.btnProperties.Click += delegate (object o, RoutedEventArgs args)
            {
                bubbleCanvas.Children.Clear();
                var propEditor = new XmlTreeView.AssetBuilderPropertiesEditor();
                propEditor.Margin = new Thickness(40);
                propEditor.Closed += delegate (object eo, EventArgs eargs)
                {
                    bubbleCanvas.Children.Clear();
                    enableForm();
                };
                FullPanel.Children.Add(propEditor);
            };
            lc.btnCancel.Click += delegate(object o, RoutedEventArgs args)
            {
                bubbleCanvas.Children.Clear();
                enableForm();
            };
            lc.btnOK.Click += delegate(object o, RoutedEventArgs args)
            {
                var update = Settings.Default.TextAssetLocation != lc.txtAddress.Text ||
                    Settings.Default.EncyclopaediaLink != lc.txtDisplay.Text ||
                    add.Any(f => Settings.Default[f.Key].ToString() != ((f.Value as TextBox)?.Text ?? (f.Value as PasswordBox)?.Password));
                if (update)
                {
                    Settings.Default.TextAssetLocation = lc.txtAddress.Text;
                    Settings.Default.EncyclopaediaLink = lc.txtDisplay.Text;
                    foreach (var item in add)
                    {
                        Settings.Default[item.Key] = (item.Value as TextBox)?.Text ?? (item.Value as PasswordBox)?.Password;
                    }
                    Settings.Default.Save();
                }
                bubbleCanvas.Children.Clear();
                enableForm();
            };
        }

        public void Change_ShowTranslation(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbShowTranslation.IsChecked)
            {
                try
                {
                    DataAccess.getLanguage(null, "");
                }
                catch
                {
                    MessageBoxResult mbr = MessageBox.Show("Alternate languages are not available on this server.", "Warning!!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    rtbShowTranslation.IsChecked = false;
                    TranslationLanguage = "";
                    return;
                }
            }
            if ((bool)rtbShowTranslation.IsChecked)
            {
                Resources["LanguageColumn"] = new GridLength(1, GridUnitType.Star);
                Resources["LanguageVisibility"] = Visibility.Visible;
                rtbShowTranslation.SmallImageSource = new BitmapImage(new Uri("images/EnableLanguage16x16.png", UriKind.Relative));
                rtbShowTranslation.LargeImageSource = new BitmapImage(new Uri("images/EnableLanguage32x32.png", UriKind.Relative));
            }
            else
            {
                rtbTranslation.IsChecked = false;
                Resources["LanguageColumn"] = new GridLength(0, GridUnitType.Star);
                Resources["LanguageVisibility"] = Visibility.Collapsed;
                rtbShowTranslation.SmallImageSource = new BitmapImage(new Uri("images/DisableLanguage16x16.png", UriKind.Relative));
                rtbShowTranslation.LargeImageSource = new BitmapImage(new Uri("images/DisableLanguage32x32.png", UriKind.Relative));
            }
            TranslationLanguage = (bool)rtbShowTranslation.IsChecked ? (string)rtbLanguage.SelectedItem : "";
            if (!(sender is string && (string)sender == "Stop")) Change_Translation(null, null);
        }

        private void Change_Translation(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbTranslation.IsChecked)
            {
                try
                {
                    DataAccess.getLanguage(null, "");
                }
                catch
                {
                    MessageBoxResult mbr = MessageBox.Show("Alternate languages are not available on this server.", "Warning!!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    rtbTranslation.IsChecked = false;
                    TranslationLanguage = "";
                    return;
                }
            }
            if ((bool)rtbTranslation.IsChecked)
            {
                rtbShowTranslation.IsChecked = true;
                Change_ShowTranslation("Stop", null);
                rtbTranslation.SmallImageSource = new BitmapImage(new Uri("images/EnableEditTranslation16x16.png", UriKind.Relative));
                rtbTranslation.LargeImageSource = new BitmapImage(new Uri("images/EnableEditTranslation32x32.png", UriKind.Relative));
            }
            else
            {
                rtbTranslation.SmallImageSource = new BitmapImage(new Uri("images/DisableEditTranslation16x16.png", UriKind.Relative));
                rtbTranslation.LargeImageSource = new BitmapImage(new Uri("images/DisableEditTranslation32x32.png", UriKind.Relative));
            }
            EditTranslation = (bool)rtbTranslation.IsChecked;
            qcat1.SetButtons();
            qcat1.SetCategoryLanguage();
            if (qcat1.LoadedAsset != null) qcat1.LoadedAsset.Refresh();
        }

        private void Update_WebService(object sender, ExecutedRoutedEventArgs e)
        {
            HideBrowsers();
            if (AlgoLoader.AlgoLoaderForm != null && AlgoLoader.AlgoLoaderForm.IsLoaded)
                AlgoLoader.AlgoLoaderForm.Close();
            if (TableEdit.TableEditForm != null && TableEdit.TableEditForm.IsLoaded)
                TableEdit.TableEditForm.Close();
            foreach (var window in Application.Current.Windows)
            {
                if(!(window is Window1)) (window as Window)?.Close();
            }
            TraversalClient = null;
            TraversalFinder = null;
            assetCanvas.Children.Clear();
            //rtbTranslation.IsChecked = false;
            //Change_Translation(null, null);
            rtbShowTranslation.IsChecked = false;
            Change_ShowTranslation(null, null);
            DataAccess.categoryLookup = true;
            LoginWebService();
        }

        private void Update_Application(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            bool updateavailable = ad.CheckForUpdate(false);
            if (updateavailable)
            {
                disableForm();
                ad.Update();
                Close();
                System.Windows.Forms.Application.Restart();
            }
            else System.Windows.Forms.MessageBox.Show("Application is up to date.");
        }

        private void Update_Groups(object sender, ExecutedRoutedEventArgs e)
        {
            XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                "@xml", qcat1.GetAssetXml("updategroups").OuterXml
            }, false);
            if (xn.Name.LocalName == "Error")
                System.Windows.Forms.MessageBox.Show(string.Format("Groups update failed.\n\n{0}", xn.Value), "Error");
            else
                System.Windows.Forms.MessageBox.Show("Groups are up to date.", "Information");
        }

        private void Priority_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbPriority.IsChecked)
            {
                rtbPriority.SmallImageSource = new BitmapImage(new Uri("images/EnablePriority_32x32.png", UriKind.Relative));
                rtbPriority.LargeImageSource = new BitmapImage(new Uri("images/EnablePriority_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbPriority.SmallImageSource = new BitmapImage(new Uri("images/NoEnablePriority_32x32.png", UriKind.Relative));
                rtbPriority.LargeImageSource = new BitmapImage(new Uri("images/NoEnablePriority_32x32.png", UriKind.Relative));
            }
            PriorityEnabled = (bool)rtbPriority.IsChecked;
            qcat1.SetButtons();
        }

        private void Category_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((bool)rtbCategory.IsChecked)
            {
                rtbCategory.SmallImageSource = new BitmapImage(new Uri("images/Enable_Move_32x32.png", UriKind.Relative));
                rtbCategory.LargeImageSource = new BitmapImage(new Uri("images/Enable_Move_32x32.png", UriKind.Relative));
            }
            else
            {
                rtbCategory.SmallImageSource = new BitmapImage(new Uri("images/Disable_Move_32x32.png", UriKind.Relative));
                rtbCategory.LargeImageSource = new BitmapImage(new Uri("images/Disable_Move_32x32.png", UriKind.Relative));
            }
            CategoryEnabled = (bool)rtbCategory.IsChecked;
            qcat1.SetButtons();
            qcat1.AddContextMenus(qcat1.AssetTypeId);
        }

        private void listButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            qcat1.CloseList();
        }

        private void listButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<int, string[]> defaultFields = new Dictionary<int, string[]>();
            defaultFields.Add(1, new string[] { "Algo_Name" });
            defaultFields.Add(2, new string[] { "Clinical_Statement", "Lay_Statement", "Question" });
            defaultFields.Add(3, new string[] { "Clinical_Answer", "Lay_Answer", "Answer_Text" });
            defaultFields.Add(4, new string[] { "Possible_Condition", "Lay_Condition" });
            defaultFields.Add(5, new string[] { "BP_TEXT" });
            defaultFields.Add(11, new string[] { "KeyName" });

            Dictionary<int, string[]> booleans = new Dictionary<int, string[]>();
            booleans.Add(3, new string[] { "StoreIfNeg" });
            booleans.Add(4, new string[] { "Silent", "Information" });

            Dictionary<int, string[]> dates = new Dictionary<int, string[]>();
            dates.Add(1, new string[] { "Date_Last_Reviewed" });

            XmlNode xe = DataAccess.getDataNode("ab_GetAsset", new string[] {
                "@AssetTypeID", qcat1.AssetTypeId.ToString(),
                "@AssetID", "new"
            }, false);

            AssetControls.assetControl.setNew(xe, qcat1);
            string[] values = listTextBox.Text.Replace("\r", "").Split('\n');
            string[] dv = defaultFields[qcat1.AssetTypeId];

            if (booleans.ContainsKey(qcat1.AssetTypeId))
                foreach (var field in booleans[qcat1.AssetTypeId])
                    AssetControls.assetControl.fixBoolean(xe, field);

            if (dates.ContainsKey(qcat1.AssetTypeId))
                foreach (var field in dates[qcat1.AssetTypeId])
                    AssetControls.assetControl.fixDate(xe, field);

            XmlNode asset = xe["Table"].Clone();
            xe.RemoveAll();

            foreach (string value in values)
            {
                if (value != "")
                {
                    XmlNode xn = asset.Clone();
                    foreach (var field in dv)
                    {
                        xn[field].InnerText = value;
                    }
                    xe.AppendChild(xn);
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xe.OuterXml);
            AssetControls.assetControl.RunUpdate(doc);
            //MessageBox.Show(doc.OuterXml);

            qcat1.CloseList();
            qcat1.Repopulate(-1);
        }

        private void Algo_Management(object sender, ExecutedRoutedEventArgs e)
        {
            AlgoLoader al;
            if (AlgoLoader.AlgoLoaderForm == null || AlgoLoader.AlgoLoaderForm.IsLoaded == false)
                al = new AlgoLoader();
            else
                al = AlgoLoader.AlgoLoaderForm;
            al.Title = Title;
            al.Show();
            al.Topmost = true;
            al.Topmost = false;
            al.Focus();
        }

        private void Cut_Click(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(qcat1.Form);
            if (ie != null)
            {
                if (ie is TextBox)
                {
                    TextBox t = ie as TextBox;
                    t.Cut();
                }
            }
        }

        public void Copy()
        {
            Copy_Click(null, null);
        }

        private void Copy_Click(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(qcat1.Form);
            if (ie != null)
            {
                if (ie is TextBox)
                {
                    TextBox t = ie as TextBox;
                    t.Copy();
                }
                if (ie is ListBoxItem)
                {
                    ie = AssetBuilder.Classes.ControlTree.getParent<ListBox>(ie as ListBoxItem);
                }
                if (ie is ListBox)
                {
                    ListBox lb = ie as ListBox;
                    if (lb != null)
                    {
                        string clip = "";
                        foreach (var item in lb.SelectedItems)
                        {
                            if (clip != "") clip += Environment.NewLine;
                            if (item is ListItem)
                                clip += (item as ListItem).ToCopyString();
                            else
                                clip += item.ToString();
                        }
                        Clipboard.SetText(clip);
                    }
                }
            }
        }

        private void Paste_Click(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(qcat1.Form);
            if (ie != null)
            {
                if (ie is TextBox)
                {
                    TextBox t = ie as TextBox;
                    t.Paste();
                }
            }
        }

        bool canInsert = false;

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (qcat1 == null) return;
            canInsert = false;
            //insertCommand.IsEnabled = false;
            //insertCommand.ToolTip = "You must first put your cursor in the place where you want to insert something.";
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(qcat1.Form);
            if (ie != null && ie is TextBox && !(ie as TextBox).IsReadOnly)
            {
                TextBox t = ie as TextBox;
                if (t.MaxLength == 0 || t.MaxLength > 100) canInsert = true;
                //if (t.MaxLength > 100)
                //{
                //    insertCommand.IsEnabled = true;
                //    insertCommand.ToolTip = "Inserts content into an asset";
                //}
                if (t.SelectionLength > 0) e.CanExecute = true;
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (qcat1 == null) return;
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(qcat1.Form);
            if (ie != null && ie is Control)
            {
                if (ie is TextBox)
                {
                    TextBox t = ie as TextBox;
                    if (t.SelectionLength > 0) e.CanExecute = true;
                }
                if (ie is ListBoxItem)
                {
                    ie = AssetBuilder.Classes.ControlTree.getParent<ListBox>(ie as ListBoxItem);
                }
                if (ie is ListBox)
                {
                    ListBox lb = ie as ListBox;
                    if (lb != null && lb.SelectedItems.Count > 0) e.CanExecute = true;
                }
            }
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (qcat1 == null) return;
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(qcat1.Form);
            if (ie != null && ie is TextBox && !(ie as TextBox).IsReadOnly)
                e.CanExecute = true;
        }

        public static RibbonGalleryItem findItem(RibbonComboBox cb, string text)
        {
            foreach (var gallery in cb.Items)
            {
                if (gallery is RibbonGallery)
                {
                    foreach (var category in (gallery as RibbonGallery).Items)
                    {
                        if (category is RibbonGalleryCategory)
                        {
                            foreach (var item in (category as RibbonGalleryCategory).Items)
                            {
                                if (item is RibbonGalleryItem && (item as RibbonGalleryItem).Content.ToString() == text) return (item as RibbonGalleryItem);
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void AddLanguage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (rtbLanguage.Text != "" && !rtbLanguage.Items.Contains(rtbLanguage.Text)) e.CanExecute = true;
        }

        private void AddLanguage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string item = rtbLanguage.Text;
            rtbLanguage.Items.Add(item);
            rtbLanguage.SelectedItem = item;
            addSettingsValue("AvailableLanguages", rtbLanguage.Text);
        }

        public void ForceLanguage(string Language)
        {
            if (!rtbLanguage.Items.Contains(Language))
            {
                rtbLanguage.Text = Language;
                AddLanguage_Executed(null, null);
            }
            else rtbLanguage.SelectedItem = Language;
            rtbShowTranslation.IsChecked = true;
            Change_ShowTranslation(null, null);
        }

        private void DeleteLanguage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (rtbLanguage.SelectedItem != null && rtbLanguage.Items.Contains(rtbLanguage.Text)) e.CanExecute = true;
        }

        private void DeleteLanguage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbLanguage.Items.Remove(rtbLanguage.Text);
            rtbLanguage.Text = "";
            updateSettingsValue("AvailableLanguages", rtbLanguage.Items);
        }

        private void IsNotEditMode(object sender, CanExecuteRoutedEventArgs e)
        {
            if (qcat1 == null || !qcat1.IsEditing) e.CanExecute = true;
        }

        private void SingleQuestionSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            if (qcat1 != null && qcat1.AssetTypeId == 2 && qcat1.LoadedAsset != null) e.CanExecute = true;
        }

        private void AssetLoaded(object sender, CanExecuteRoutedEventArgs e)
        {
            if (qcat1 != null && qcat1.AssetTypeId > 0  && qcat1.AssetTypeId < 6 && qcat1.LoadedAsset != null) e.CanExecute = true;
        }

        private void IsValidated(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((bool)rtbValidation.IsChecked) e.CanExecute = true;
        }

        private void CanTranslate(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((qcat1 == null || !qcat1.IsEditing) && rtbLanguage != null && !string.IsNullOrWhiteSpace(rtbLanguage.Text)) e.CanExecute = true;
        }

        private void rgLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Selection changed to " + rtbLanguage.SelectedItem);
            Change_ShowTranslation(null, null);
        }

        private void AlgoTracker_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AlgoTracker at = new AlgoTracker();
            at.Show();
            //Process.Start("AssetBuilder:NewTrack.");
        }

        private void RiskCalc_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RiskCalc.MainWindow rc = new RiskCalc.MainWindow();
            rc.WindowState = System.Windows.WindowState.Maximized;
            rc.Show();
        }

        private void Properties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (qcat1.LoadedAsset != null && qcat1.AssetTypeId > 0 && qcat1.LoadedAsset.AssetID.HasValue)
            {
                var props = Controls.Properties.CreateProperties((AssetType)qcat1.AssetTypeId, qcat1.LoadedAsset.AssetID.Value.ToString());
                //props.WindowState = System.Windows.WindowState.Maximized;
                props.Show();
            }
            //else if(qcat1.loadedAsset == null)
            //{
            //    var props = AssetBuilder.Controls.Properties.CreateProperties(AssetControls.AssetType.AssetBuilder, "Properties");
            //    props.Show();
            //}
        }

        private void LaunchHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LaunchHelp();
        }

        //private void rgLanguage_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (rtbLanguage.Text != "" && !rtbLanguage.Items.Contains(rtbLanguage.Text)) AddLanguage_Executed(null, null);
        //}

        private void listTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            qcat1.UpdateAdorner(sender as TextBox);
        }

        private void RibbonWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                LaunchHelp();
            }
        }

        private static void LaunchHelp()
        {
            string url = Properties.Settings.Default.HelpUrl;
            url = string.Format("{0}#New_Topic.htm", url);
            Process.Start(url);
        }

        private void rgSpellingLanguage_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DefaultLanguage = (System.Windows.Markup.XmlLanguage)rgSpellingLanguage.SelectedValue;
            if (assetCanvas.Children.Count > 0)
            {
                setLanguage(((AssetControls.assetControl)assetCanvas.Children[0]).AssetDockPanel.Children);
            }
        }

        void setLanguage(UIElementCollection vc)
        {
            foreach (UIElement item in vc)
            {
                if (item is TextBox)
                {
                    TextBox tb = item as TextBox;
                    if ((bool)tb.GetValue(SpellCheck.IsEnabledProperty))
                    {
                        tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                        tb.SetValue(TextBox.LanguageProperty, DefaultLanguage);
                    }
                }
                if (item is Panel) setLanguage((item as Panel).Children);
            }
        }

        private void searchType_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter.ToString() != "-1" || Window1.ShowTranslation) e.CanExecute = true;
            else
            {
                if ((bool)rtbSearchTranslation.IsChecked || Window1.SearchTranslation)
                {
                    DisableSearchTranslation();
                }
            }
        }

        public void DisableSearchTranslation()
        {
            SearchTranslation = false;
            rtbSearchTranslation.IsChecked = false;
            rtbSearchTranslation.SmallImageSource = new BitmapImage(new Uri("images/DisableLanguage16x16.png", UriKind.Relative));
            rtbSearchTranslation.LargeImageSource = new BitmapImage(new Uri("images/DisableLanguage32x32.png", UriKind.Relative));
        }

        private void Debug_Application(object sender, ExecutedRoutedEventArgs e)
        {
            if (DebugOutput.DebugOuputForm != null && DebugOutput.DebugOuputForm.IsLoaded)
                DebugOutput.DebugOuputForm.Close();
            DebugOutput dbg = new DebugOutput();
            dbg.Show();
        }

        private async void Compare_Versions(object sender, ExecutedRoutedEventArgs e)
        {
            HideBrowsers();
            Compare c = await Compare.Create();
            //c.Margin = new Thickness(0);
            qcat1.Form.disableForm();
            FullPanel.Children.Add(c);
        }

        private void UserManagement_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(_userManagementWindow != null && _userManagementWindow.IsLoaded))
            {
                _userManagementWindow = new UserManagementWindow();
            }
            
            var viewModel = new UserManagementWindowViewModel(UserName);
            _userManagementWindow.DataContext = viewModel;
            _userManagementWindow.Show();

            _userManagementWindow.Topmost = true;
            _userManagementWindow.Topmost = false;
            _userManagementWindow.Focus();
        }

        private void TableEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TableEdit w;
            if (TableEdit.TableEditForm == null || TableEdit.TableEditForm.IsLoaded == false)
                w = new TableEdit();
            else
                w = TableEdit.TableEditForm;
            w.Title = Title;
            w.WindowState = WindowState.Maximized;
            w.Show();
            w.Topmost = true;
            w.Topmost = false;
            w.Focus();
        }

        private void CodeAssociation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CodeAssociation.CodeAssociation ca = new CodeAssociation.CodeAssociation();
            ca.Show();
        }

        public void Start_TraversalClient(object prm)
        {
            Uri source = null;
            if (prm != null && prm is Tuple<string, string>)
            {
                var p = prm as Tuple<string, string>;
                source = new Uri(new Uri(Settings.Default.WebService), $"TraversalService/Traversal.html?builder=1&algoid={p.Item1}&nodeid={p.Item2}&memberid=-1");
                if (TraversalClient != null) TraversalClient.Source = source;
            }
            else if (TraversalClient == null)
                source = new Uri(new Uri(Settings.Default.WebService), "TraversalService/Traversal.html?builder=1");

            if (TraversalClient == null)
            {
                TraversalClient = new WebView2
                {
                    Source = source,
                    AllowDrop = false
                };
                TraversalClient.NavigationStarting += (o, args) =>
                {
                    var uri = new Uri(args.Uri);
                    if (uri.Scheme == "assetbuilder")
                    {
                        string[] prms = uri.AbsolutePath.Split('.');
                        int nt = Program.GetNodeType(prms[0]);
                        int id;
                        if (prms.Length > 1 && int.TryParse(prms[1], out id)) OpenAsset(nt, id.ToString());
                        else if (nt == -1)
                        {
                            new Controls.AlgoTracker { Url = string.Join(".", prms.Skip(1)) }.Show();
                        }
                        else if (nt == -2)
                        {
                            new Controls.AlgoTracker().Show();
                        }
                        args.Cancel = true;
                    }
                };                
            }
            UncheckToggles(rtbTraversalClient);
            HideBrowsers("TraversalClient");
            if (rtbTraversalClient.IsChecked == true) FullPanel.Children.Add(TraversalClient);
            else FullPanel.Children.Remove(TraversalClient);
        }

        private void TraversalClient_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Start_TraversalClient(e.Parameter);
        }

        public void Start_TraversalFinder()
        {
            Uri source = new Uri(new Uri(Settings.Default.WebService), "TraversalService/TableOutput/ab_builderdefaults/json");
            if (TraversalFinder == null)
            {
                TraversalFinder = new WebView2
                {
                    AllowDrop = false
                };
                var str = System.IO.File.ReadAllText("Html/Finder.html").Replace("$$URL$$", source.AbsoluteUri);
                File.WriteAllText("Html/RunningFinder.html", str);
                TraversalFinder.Source = new Uri(Path.Combine(Environment.CurrentDirectory, @"Html\RunningFinder.html"));
                //TraversalFinder.NavigateToString(str);                
            }
            UncheckToggles(rtbTraversalFinder);
            HideBrowsers("TraversalFinder");
            if (rtbTraversalFinder.IsChecked == true) FullPanel.Children.Add(TraversalFinder);
            else FullPanel.Children.Remove(TraversalFinder);
        }

        private void UncheckToggles(RibbonToggleButton rtb)
        {
            if (!rtb.IsChecked == true) return;
            foreach (var item in ((RibbonGroup)rtb.Parent).Items)
            {
                if (item is RibbonToggleButton && item != rtb && (item as RibbonToggleButton).IsChecked == true)
                    (item as RibbonToggleButton).IsChecked = false;
            }
        }

        private void TraversalFinder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Start_TraversalFinder();
        }

        private void EnableLanguageInheritance_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (qcat1 == null || !qcat1.IsEditing) && AlternateLanguages != null && AlternateLanguages.Count > 0;
        }

        private void Change_EnableLanguageInheritance(object sender, ExecutedRoutedEventArgs e)
        {
            SetLanguageIheritanceButtonImage();
            Settings.Default.HideTranslationInheritance = (bool)rtbEnableLanguageInheritance.IsChecked;
            saveSettings();
            EnableLanguageInheritance = (bool)rtbEnableLanguageInheritance.IsChecked;

        }

        private void SetLanguageIheritanceButtonImage()
        {
            if ((bool)rtbEnableLanguageInheritance.IsChecked)
            {
                rtbEnableLanguageInheritance.SmallImageSource = new BitmapImage(new Uri("images/EnableEditTranslation16x16.png", UriKind.Relative));
                rtbEnableLanguageInheritance.LargeImageSource = new BitmapImage(new Uri("images/EnableEditTranslation32x32.png", UriKind.Relative));
            }
            else
            {
                rtbEnableLanguageInheritance.SmallImageSource = new BitmapImage(new Uri("images/DisableEditTranslation16x16.png", UriKind.Relative));
                rtbEnableLanguageInheritance.LargeImageSource = new BitmapImage(new Uri("images/DisableEditTranslation32x32.png", UriKind.Relative));
            }
        }

        private void rgLanguage_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (rtbLanguage.Text != "" && !rtbLanguage.Items.Contains(rtbLanguage.Text)) AddLanguage_Executed(null, null);
        }

        private void btnTextError_Click(object sender, RoutedEventArgs e)
        {
            throw new Exception("Test error message from Asset Builder");
        }
    }
}
