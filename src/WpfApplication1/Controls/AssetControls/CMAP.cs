using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Xml;

namespace AssetBuilder.AssetControls
{
    public class CMAP : assetControl
    {
        string updateRelease = "";
        ConclusionMap.Classes.Map _cmap { get; set; } = new ConclusionMap.Classes.Map();

        public ConclusionMap.Classes.Map ConclusionMap { get { return _cmap; } }

        static CMAP()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CMAP), new FrameworkPropertyMetadata(typeof(CMAP)));
        }

        public override bool Save(bool toSaas = true)
        {
            ConclusionMap.name = asset["Table"]["MapName"].InnerText;
            asset["Table"]["Release"].InnerText = ConclusionMap.releaseNumber = updateRelease;
            ConclusionMap.id = AssetID ?? -1;
            asset["Table"]["MapData"].InnerText = (new JavaScriptSerializer()).Serialize(ConclusionMap);
            var b = base.Save();
            if (b) _cmap.EditMode = !b;
            return b;
        }

        public override void goEdit()
        {
            base.goEdit();
            _cmap.EditMode = true;
        }

        public override void Cancel()
        {
            base.Cancel();
            try
            {
                _cmap = (new JavaScriptSerializer()).Deserialize<ConclusionMap.Classes.Map>(asset["Table"]["MapData"].InnerText);
                _cmap.EditMode = false;
            }
            catch (Exception ex)
            {
                _cmap = new ConclusionMap.Classes.Map();
                MessageBox.Show($"Invalid Conclusion Map Data\n\n{ex.Message}", "Error in asset load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            (GetTemplateChild("MapControl") as ConclusionMap.Controls.Map).DataContext = ConclusionMap;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            (GetTemplateChild("MapControl") as ConclusionMap.Controls.Map).DataContext = ConclusionMap;
            //ConclusionMap.GlobalUpdate = () =>
            //{
            //    asset["Table"]["MapData"].InnerText = (new JavaScriptSerializer()).Serialize(ConclusionMap);
            //};
            ButtonChildren["btnAddRule"].Click += new RoutedEventHandler(AddItem);
            bs.ReleaseConclusionMap += Bs_ReleaseConclusionMap;
        }

        private void Bs_ReleaseConclusionMap(object sender, RoutedEventArgs e)
        {
            var c = Properties.Settings.Default.ClientID;
            var s = Properties.Settings.Default.Secret;
            var identity = new Uri(Properties.Settings.Default.SaaSIdentity);
            var product = new Uri(Properties.Settings.Default.SaaSEndpoint);
            var getToken = $"grant_type=client_credentials&client_id={c}&client_secret={s}";
            var j = getToken.PostObject<JNode>(new Uri(identity, "connect/token").AbsoluteUri, new[] { ("Content-Type", "application/x-www-form-urlencoded") });

            var headers = new[] {
                ("Content-Type", "application/json"),
                ("Authorization", $"Bearer {j["access_token"].Value}"),
            };

            var get = new Uri(product, $"api/v1/{c}/conclusionmaps/GetConclusionMapByAssetId/{ConclusionMap.id}").AbsoluteUri.GetContent<JNode>(headers);
            var id = get["id"].Value;
            var verb = " updated";
            var result = "";

            if (id == null)
            {
                var create = new { name = ConclusionMap.name, assetId = ConclusionMap.id };
                var t = create.PostObject<JNode>(new Uri(product, $"api/v1/{c}/conclusionmaps").AbsoluteUri, headers);
                id = t["id"].Value;
                verb = " created";
            }
            if (id != null)
            {
                var release = ConclusionMap.PostObject<JNode>(new Uri(product, $"api/v1/{c}/conclusionmaps/{id}/releases").AbsoluteUri, headers);
                result = $"Release {release["releaseNumber"].Value} created on {release["createdDate"].Value}";
                if (release["status"].Value != null)
                {
                    result = $"Release {release["releaseNumber"].Value} failed with status code {release["status"].Value}";
                }
            }
            else
            {
                verb = "failed";
                result = "ConclusionMap Name already exists.";
            }
            MessageBox.Show($"ConclusionMap {id}{verb}\n\n{result}");
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            ConclusionMap.rules.Add(new ConclusionMap.Classes.Rule());
        }

        public CMAP()
        {

        }

        public CMAP(XmlNode map) : base(map)
        {
            try
            {
                _cmap = (new JavaScriptSerializer()).Deserialize<ConclusionMap.Classes.Map>(map["Table"]["MapData"].InnerText);
                if (_cmap == null) _cmap = new ConclusionMap.Classes.Map();
                if (_cmap.rules == null) _cmap.rules = new System.Collections.ObjectModel.ObservableCollection<ConclusionMap.Classes.Rule>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid Conclusion Map Data\n\n{ex.Message}", "Error in asset load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            assetType = AssetType.ConclusionMap;
            tableName = "CMAP";
            expert = map["Table"]["MapName"];
            SetRelease(map["Table"]);
            cats.Add(0, "DataID");
            cats.Add(1, "CatID");
            cats.Add(2, "SubCatID");
            cats.Add(3, "Cat2ID");
        }

        private void SetRelease(XmlNode map)
        {
            var date = DateTime.Now;
            var q = (date.Month - 1) / 3 + 1;
            var y = date.Year;
            updateRelease = map["Release"]?.InnerText ?? "";
            var split = updateRelease.Split('.');
            int i;
            if (updateRelease.StartsWith($"{y}.{q:D2}") && split.Length > 2 && int.TryParse(split[2], out i)) updateRelease = $"{y}.{q:D2}.{i + 1:D2}";
            else updateRelease = $"{y}.{q:D2}.01";
            if (string.IsNullOrWhiteSpace(map["Release"].InnerText)) map["Release"].InnerText = updateRelease;
        }

        private void setButtons()
        {
            var b = EditMode;
            foreach (var item in AssetBuilder.Classes.ControlTree.getChildren<System.Windows.Controls.Button>(GetTemplateChild("MapControl")))
            {
                item.IsEnabled = b;
            }
        }
    }
}
