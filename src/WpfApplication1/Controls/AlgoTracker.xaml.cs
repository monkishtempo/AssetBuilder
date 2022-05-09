using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using AssetBuilder.AssetControls;
using AssetBuilder.Classes;
using AssetBuilder.Properties;
using Microsoft.Windows.Controls.Ribbon;
using Extensions = CryptoExtension;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for AlgoTracker.xaml
    /// </summary>
    public partial class AlgoTracker : ABRibbonWindow
    {
        public Dictionary<string, List<ABEnvironment>> Environments
        {
            get { return (Dictionary<string, List<ABEnvironment>>)GetValue(EnvironmentsProperty); }
            set { SetValue(EnvironmentsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Environments.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnvironmentsProperty =
            DependencyProperty.Register("Environments", typeof(Dictionary<string, List<ABEnvironment>>), typeof(Window1), new UIPropertyMetadata());

        internal XElement LoadedAlgos;
        private string _traversalId;
        private readonly Dictionary<string, string> _webbuilders;
        private string _remoteUrl;
        private bool newengine = false;

        public Color TrackingColour = Colors.Red;

        public string NodeData64
        {
            set
            {
                textBox1.Text = "From Session data" + Environment.NewLine + UTF8Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
        }

        public string Url
        {
            set
            {
                string s = Extension.GetWebRequest(value);
                if (value.IndexOf("/traversalservice", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    var url = new Uri(value.Substring(0, value.IndexOf("/traversalservice", StringComparison.InvariantCultureIgnoreCase)) + "/Data.asmx").AbsoluteUri;
                    if (string.Compare(url, Settings.Default.WebService, true) != 0)
                    {
                        _remoteUrl = url;
                    }
                }
                if (s.IsBase64String())
                {
                    NodeData64 = s;
                }
                traversalurlorguid.Text = value;
            }
        }

        public AlgoTracker()
        {
            InitializeComponent();
            DataAccess.ClearEnvironments();
            Environments = DataAccess.Environments;
            _webbuilders = DataAccess.WebBuilders;
            ddlEnvironments.ItemsSource = Environments.Select(f => f.Key);
            // Insert code required on object creation below this point.
            TrackingColourMenu.LargeImageSource = GetColourImage(Brushes.Red);
            rbtColourTrackRed.ImageSource = GetColourImage(Brushes.Red);
            rbtColourTrackGreen.ImageSource = GetColourImage(Brushes.Lime);
            rbtColourTrackBlue.ImageSource = GetColourImage(Brushes.Blue);
        }

        private DrawingImage GetColourImage(SolidColorBrush colour)
        {
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 32, 32));
                dc.DrawRectangle(colour, null, new Rect(3, 3, 26, 26));
            }
            return new DrawingImage(dv.Drawing);
        }

        private void rbtColour_Click(object sender, RoutedEventArgs e)
        {
            RibbonMenuItem rbi = sender as RibbonMenuItem;
            string colour = rbi.Header.ToString() == "Green" ? "Lime" : rbi.Header.ToString();
            if (colour != "Custom") TrackingColour = (Color)ColorConverter.ConvertFromString(colour);
            else
            {
                var cd = new System.Windows.Forms.ColorDialog();
                var result = cd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) TrackingColour = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
            }
            TrackingColourMenu.LargeImageSource = GetColourImage(new SolidColorBrush(TrackingColour));
        }

        private void ribbonButton1_Click(object sender, RoutedEventArgs e)
        {
            var ids = new HashSet<int>();
            foreach (var item in algos.SelectedItems)
            {
                int i = 0;
                var ali = item as AlgoListItem;
                if (item != null && int.TryParse(ali.ID, out i))
                    ids.Add(i);
            }
            var p = Classes.VisioInterface.Track(textBox1.Text, ids, TrackingColour);
            if (p != null)
            {
                //var ps = PresentationSource.FromVisual(this);
                //var dpi = ps.CompositionTarget.TransformFromDevice.M11;
                p.Owner = this;
                p.ShowDialog();
            }
        }

        void setButtons(bool forceDisable = false)
        {
            if (forceDisable)
            {
                rbtGetRemote.IsEnabled = false;
                rbtGet.IsEnabled = false;
            }
            else
            {
                bool remote = _remoteUrl != null && !_remoteUrl.ToLower().StartsWith(DataAccess.Domain);
                rbtGetRemote.IsEnabled = remote;
                rbtGet.IsEnabled = !remote;
            }
        }

        static char[] chrs = "?=&/".ToCharArray();
        static char[] ochrs = "?=&".ToCharArray();

        private void input__TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _traversalId = null;
            rbtCopy.IsEnabled = false;
            rbtGet.IsEnabled = false;
            setButtons(true);
            ddlEnvironments.SelectedItem = null;
            string[] split = traversalurlorguid.Text.Split(chrs);
            foreach (var item in split)
            {
                string test = item.Trim();
                if (test.Length >= 56)
                {
                    if (!test.IsBase64String()) test = HttpUtility.UrlDecode(test);
                    try
                    {
                        if (test.IsBase64String())
                        {
                            var vanilla = Encoding.UTF8.GetString(Convert.FromBase64String(test));
                            Guid b64 = Guid.Empty;
                            if (vanilla.Split('|').Any(f => f.Length == 36 && Guid.TryParse(f, out b64)))
                                test = b64.ToString();
                            else test = test.Decrypt();
                        }
                    }
                    catch (Exception ex) { textBox1.Text = ex.ToString(); }
                }

                Guid g;
                if (test.Length == 36 && Guid.TryParseExact(test, "D", out g))
                {
                    setButtons(true);
                    var s0 = traversalurlorguid.Text.Split(ochrs)[0];
                    _traversalId = g.ToString();
                    rbtCopy.IsEnabled = true;
                    rbtGet.IsEnabled = true;
                    if (s0.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var sp = s0.Split('/');
                        var host = sp.Length > 2 ? sp[2] : "";
                        Uri u = null;
                        if (Regex.IsMatch(s0, "traversal\\/[0-9a-f]{8}(-[0-9a-f]{4}){3}(-[0-9a-f]{12})$", RegexOptions.IgnoreCase))
                        {
                            u = new Uri(s0.Substring(0, s0.IndexOf("/traversal", StringComparison.InvariantCultureIgnoreCase)).Replace("client-", "api-") + "/index.html");
                        }
                        else if (s0.IndexOf("/traversalservice", StringComparison.InvariantCultureIgnoreCase) > -1)
                            u = new Uri(s0.Substring(0, s0.IndexOf("/traversalservice", StringComparison.InvariantCultureIgnoreCase)) + "/Data.asmx");
                        else
                            u = _webbuilders.ContainsKey(host) ? new Uri(_webbuilders[host]) : new Uri(new Uri(s0), "/WebBuilder/Data.asmx");
                        LookupEnvironment(u.OriginalString);
                        setButtons();
                    }
                    lblWebService.Content = _remoteUrl;
                    if (_remoteUrl != null) GetLinks(_traversalId);
                }
            }
        }

        private bool LookupEnvironment(string url)
        {
            _remoteUrl = url;
            newengine = false;
            WebRequest web = WebRequest.Create(url);
            web.Timeout = 5000;
            try
            {
                using (var resp = (HttpWebResponse)web.GetResponse())
                {
                    if ((int)resp.StatusCode >= 400) _remoteUrl = null;
                    resp.Close();
                }
            }
            catch (Exception ex)
            {
                _remoteUrl = null;
                textBox1.Text = ex.Message;
            }
            lblWebService.Content = _remoteUrl;
            if (_remoteUrl.EndsWith("index.html"))
            {

                return (newengine = true);
            }
            return _remoteUrl != null;
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            traversalurlorguid.SelectAll();
        }

        void GetTrackOrReRun(string guid, string url)
        {
            if (newengine)
            {
                var authUri = new Uri(new Uri(url), $"Token/GetAsync/service.client/secret");
                var auth = Extension.GetWebRequest(authUri.AbsoluteUri);
                var jAuth = JNode.CreateFromJson(auth);
                var trackUri = new Uri(new Uri(url), "Traversal/GetTrackFile/" + guid);
                var resp = Extension.GetWebRequest(trackUri.AbsoluteUri, new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {jAuth["accessToken"].Value}" }
                });
                var jNode = JNode.CreateFromJson(resp);
                if (jNode["data"] != null)
                {
                    textBox1.Text = "From new engine" + Environment.NewLine + jNode["data"];
                    greyCanvas.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                var trackUri = new Uri(new Uri(url), "TraversalService/Track/" + guid);
                var resp = Extension.GetWebRequest(trackUri.AbsoluteUri);
                if (resp != "" && resp.IsBase64String())
                {
                    NodeData64 = resp;
                    greyCanvas.Visibility = Visibility.Hidden;
                }
                else
                {
                    var t = new Task<XElement>(
                        () => DataAccess.getData("dsp_ReRunTraversal", new string[] { "@TraversalID", guid }, false, url));
                    t.ContinueWith<XElement>((t2) =>
                    {
                        DisplayTrack(t.Result);
                        return t.Result;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    t.Start();
                }
            }
        }

        private void GetTrackFile_Click(object sender, RoutedEventArgs e)
        {
            if (_traversalId != null)
            {
                greyCanvas.Visibility = Visibility.Visible;
                textBox1.Text = "";
                string guid = _traversalId;
                GetTrackOrReRun(guid, Settings.Default.WebService);
            }
        }

        private void DisplayTrack(XElement res)
        {
            if (res.Name.LocalName == "Error")
                textBox1.Text = res.Value;
            else if (res.Element("InfoMessage") != null)
                textBox1.Text = res.Element("InfoMessage").Value;
            greyCanvas.Visibility = Visibility.Hidden;
        }

        private void GetRemoteTrackFile_Click(object sender, RoutedEventArgs e)
        {
            if (_traversalId != null)
            {
                greyCanvas.Visibility = Visibility.Visible;
                textBox1.Text = "";
                string guid = _traversalId;
                var Url = _remoteUrl;
                GetTrackOrReRun(guid, Url);
            }
        }

        private void rbtCopy_Click(object sender, RoutedEventArgs e)
        {
            if (_traversalId != null) Clipboard.SetText(_traversalId);
        }

        private void textBox1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            int i = 0;
            int o = 0;
            var col = t.Text.Split('\n')
                .SelectMany(f => f.Split(',').Select(s => s.Trim()))
                .Where(f => f.StartsWith("Algo : "))
                .Select(f => f.Split(':').Last().Trim())
                .Where(f => int.TryParse(f, out i))
                .Select(f => i).Distinct().Select(f => new { AlgoID = f, Order = ++o }).ToArray();
            var list = DataAccess.getData("ab_GetItems", new[] {
                "@boxid", "0",
                "@assettypeid", "1",
                "@searchword", "",
                "@algoid", string.Join(",", col.Select(f => f.AlgoID)) + "," }, false, newengine ? null : _remoteUrl
            ).Elements().Where(f => f.ElementValue("BoxID") == "5" && f.ElementValue("ID") != null && f.ElementValue("Description") != null)
            .OrderBy(f => col.Where(c => c.AlgoID.ToString() == f.ElementValue("ID")).Select(c => c.Order).FirstOrDefault())
            .Select(f => new AlgoListItem { ID = f.ElementValue("ID"), Name = f.ElementValue("Description") }).ToArray();
            var cc = new CompositeCollection
            {
                new CollectionContainer {Collection = list.Where(l => int.TryParse(l.ID, out i) && col.Any(f => f.AlgoID == i))},
                new CollectionContainer
                {
                    Collection = col.Select(f => f.AlgoID.ToString()).Except(list.Select(f => f.ID)).Select(f => new MissingAlgoListItem { ID = f })
                }
            };
            algos.ItemsSource = cc;
        }

        private void algos_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(algos.SelectedItem is AlgoListItem)) return;
            AlgoListItem algo = (AlgoListItem)algos.SelectedItem;
            UsageShape us = new UsageShape
            {
                AlgoID = algo.ID,
                AlgoName = algo.Name,
                ShapeName = "Algo Start"
            };
            assetControl.VisioFindorLoad(us, ref LoadedAlgos, null);
        }

        private static char[] returns = "\r\n".ToCharArray();
        private static char[] breaks = ",\r\n".ToCharArray();

        private void textBox1_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            var line = getLine(tb, returns).Trim();
            //var clause = getLine(tb, breaks).Trim();
            var dict = line.Split(',')
                .Where(f => f.Contains(':'))
                .Select(f => f.Split(':')).Distinct(new KeyEqualityComparer<string[]>(f => f[0].Trim()))
                .ToDictionary(f => f[0].Trim(), f => f[1].Trim());
            UsageShape us = new UsageShape();
            if (dict.ContainsKey("Algo")) us.AlgoID = dict["Algo"];
            if (dict.ContainsKey("Node")) us.NodeID = dict["Node"];
            if (!string.IsNullOrWhiteSpace(us.AlgoID) && !string.IsNullOrWhiteSpace(us.NodeID))
                assetControl.VisioFindorLoad(us, ref LoadedAlgos, null);
        }

        private string getLine(TextBox tb, char[] c)
        {
            var s = tb.SelectionStart;
            var t = tb.Text;
            var t1 = t.Substring(0, s);
            var t2 = t.Substring(s, t.Length - s);
            var ls = t1.LastIndexOfAny(c) + 1;
            var le = t2.IndexOfAny(c);
            if (le == -1) le = t2.Length + s; else le += s;
            tb.SelectionStart = ls;
            tb.SelectionLength = le - ls;
            return t.Substring(ls, le - ls);
        }

        private void ddlEnvironments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var envs = Environments;
            var env = ddlEnvironments.SelectedItem as string;
            var traversalID = _traversalId;
            if (env != null && envs.ContainsKey(env))
            {
                var match = envs[env].Where(f => f.Type == "WebBuilder").Take(1).ToArray();
                if (match.Length == 1)
                {
                    _remoteUrl = match[0].Url;
                    lblWebService.Content = _remoteUrl;
                    LookupEnvironment(_remoteUrl);
                    setButtons();
                }
                GetLinks(envs[env], traversalID);
            }
            else links.Items.Clear();
        }

        private string getHost(string url)
        {
            var split = url.Split('/');
            return split.Length > 2 ? split[2] : null;
        }

        private void GetLinks(string traversalId)
        {
            var host = getHost(_remoteUrl);
            var match =
                Environments
                    .Where(f => f.Value.Any(w => w.Type == "WebBuilder" && w.Host == host))
                    .Take(1)
                    .ToArray();
            if (match.Length == 1)
            {
                ddlEnvironments.SelectedItem = match[0].Key;
            }
            //else if (newengine)
            //{
            //    links.Items.Clear();
            //    var ne = new Uri(new Uri(_remoteUrl), $"api/engine/");
            //    var uri = new Uri(new Uri(Settings.Default.WebService), $"traversalService/QAReport/{traversalId}?url={ne}");
            //    links.Items.Add(new ListBoxItem() { Content = "Traversal Report", Tag = (uri.AbsoluteUri, ne.AbsoluteUri) });
            //}
        }

        private void GetLinks(List<ABEnvironment> env, string traversalId)
        {
            if (!string.IsNullOrEmpty(traversalId))
            {
                links.Items.Clear();
                foreach (var val in env)
                {
                    var link = val;
                    if (val.Type != "WebBuilder")
                        links.Items.Add(new ListBoxItem() { Content = val.Type, Tag = Tuple.Create(link, traversalId) });
                }

            }

        }

        private void links_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var lb = sender as ListBox;
            if (newengine)
            {
                var lbi = lb.SelectedItem as ListBoxItem;
                //var url = ((string wburl, string ne))lbi.Tag;
                var data = (Tuple<ABEnvironment, string>)lbi.Tag;
                HtmlReport hr = new HtmlReport(data.Item1.Url);
                hr.writeTraversalReport(_traversalId);
                //Process.Start(url.wburl);
            }
            else if (lb.SelectedItem != null && lb.SelectedItem is ListBoxItem)
            {
                var lbi = lb.SelectedItem as ListBoxItem;
                var data = (Tuple<ABEnvironment, string>)lbi.Tag;
                var url = data.Item1.Url;

                var xn = DataAccess.getData("dsp_GetNaturalLanguageProperties",
                    new string[] { "@TraversalID", data.Item2 }, false, _remoteUrl);
                if (xn.Name.LocalName == "Error")
                {
                    links.Items.Clear();
                    DataAccess.AddLastCommand("GetLinks", xn.GetXmlNode(), TimeSpan.MinValue);
                    links.Items.Add(xn.Value);
                }
                XElement prms = xn.Element("Table");
                if (prms != null)
                {
                    Dictionary<string, string> replacements = new Dictionary<string, string>();
                    float f;
                    if (float.TryParse(prms.ElementValue("DaysAlive"), out f))
                        replacements.Add("DOB", DateTime.Now.AddDays(-f).ToString("yyyy-MM-dd"));
                    foreach (var prm in prms.Elements())
                    {
                        replacements.Add(prm.Name.LocalName, prms.ElementValue(prm.Name.LocalName));
                    }
                    replacements.Add("TraversalID", data.Item2);
                    url = data.Item1.GetUrl(replacements);
                    Process.Start(url);
                }
            }
        }
    }

    public class ListItemBase
    {
        public string ID { get; set; }
    }

    public class AlgoListItem : ListItemBase
    {
        public string Name { get; set; }
    }

    public class MissingAlgoListItem : ListItemBase
    {
    }
}
