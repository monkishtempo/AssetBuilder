using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using AssetBuilder.Classes;
using AssetBuilder.Controls;
using AssetBuilder.ViewModels;

namespace AssetBuilder.AssetControls
{
    public class Conclusion : assetControl
    {
        static Conclusion()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Conclusion), new FrameworkPropertyMetadata(typeof(Conclusion)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
			if (IsDesignTime) return;
			btnBullet = GetTemplateButton("ebtnBullet");
			btnBullet.Click += new RoutedEventHandler(btnBullet_Click);
			btnHideBullet = GetTemplateButton("ebtnHideBullet");
			btnHideBullet.Click += new RoutedEventHandler(btnBullet_Click);
			//if (asset["Table"]["RecID"].InnerText == "new") btnBullet.Visibility = System.Windows.Visibility.Collapsed;
			bulletPanel = GetTemplateChild("bulletPanel") as StackPanel;
            gs = GetTemplateChild("gridSplitter1") as GridSplitter;
            ButtonChildren["btnAddBullet"].Click += new RoutedEventHandler(AddBullet);
			ButtonChildren["btnRemoveBullet"].Click += new RoutedEventHandler(RemoveBullet);
			ButtonChildren["btnRemoveAllBullets"].Click += new RoutedEventHandler(RemoveAllBullets);
			ButtonChildren["btnMoveBulletUp"].Click += new RoutedEventHandler(PriorityChange);
            ButtonChildren["btnMoveBulletDown"].Click += new RoutedEventHandler(PriorityChange);
            bulletPanel.PreviewDragEnter += new DragEventHandler(bullet_PreviewDragEnter);
            bulletPanel.PreviewDragOver += new DragEventHandler(bullet_PreviewDragEnter);
            bulletPanel.PreviewDragLeave += new DragEventHandler(bulletPanel_PreviewDragLeave);
            bulletPanel.PreviewDrop += new DragEventHandler(bullet_Drop);
			bulletCount = GetTemplateChild("bulletCount") as countIndicator;

            if (Window1.McKesson_Mode)
            {
                (GetTemplateChild("lblInformation") as Label).Visibility = Visibility.Collapsed;
                (GetTemplateChild("lblWatchout") as Label).Visibility = Visibility.Visible;
                (GetTemplateChild("lblSilent") as Label).Visibility = Visibility.Collapsed;
                (GetTemplateChild("lblSelfCare") as Label).Visibility = Visibility.Visible;
                XmlNode idcolumn = asset.SelectSingleNode("Table/*");
                if (idcolumn.InnerText == "new")
                {
                    if (cat.listBox4.SelectedItem != null && (cat.listBox4.SelectedItem as ListItem).Value == "WatchoutCondition")
                        asset["Table"]["Information"].InnerText = true.ToString();
                    if (cat.listBox4.SelectedItem != null && (cat.listBox4.SelectedItem as ListItem).Value == "SelfCare")
                        asset["Table"]["Silent"].InnerText = true.ToString();
                }

            }
		}

        void bulletPanel_PreviewDragLeave(object sender, DragEventArgs e)
        {
            clearTops();
        }

        void AddBullet(object sender, RoutedEventArgs e)
        {
            cat.Form.changeAssetType(5);
        }

		void RemoveBullet(object sender, RoutedEventArgs e)
		{
			if (SelectedBullet != null)
			{
				bu.Remove(SelectedBullet);
				if (SelectedBullet.BUID == "new") SelectedBullet.Bullet.Remove();
				else SelectedBullet.Bullet.Add(new XElement("Delete"));
				SelectedBullet = null;
				ButtonChildren["btnMoveBulletUp"].IsEnabled = false;
				ButtonChildren["btnMoveBulletDown"].IsEnabled = false;
				ButtonChildren["btnRemoveBullet"].IsEnabled = false;
				drawBullets();
			}
		}

		void RemoveAllBullets(object sender, RoutedEventArgs e)
		{
			foreach (var item in bu)
			{
				if (item != null)
				{
					if (item.BUID == "new") item.Bullet.Remove();
					else item.Bullet.Add(new XElement("Delete"));
				}
			}
			
			bu.Clear();

			SelectedBullet = null;
			ButtonChildren["btnMoveBulletUp"].IsEnabled = false;
			ButtonChildren["btnMoveBulletDown"].IsEnabled = false;
			ButtonChildren["btnRemoveBullet"].IsEnabled = false;
			drawBullets();
		}

		void clearTops()
        {
            foreach (var item in bulletPanel.Children)
            {
                if (item is DependencyObject)
                {
                    BulletLine bl = getBulletLineChild(item as DependencyObject);
                    if (bl != null) bl.clearTop();
                }
            }
        }

        int blpos = 32768;

        void bullet_PreviewDragEnter(object sender, DragEventArgs e)
        {
            string data = e.Data.GetData(typeof(string)).ToString();
            clearTops();
            Point p = e.GetPosition(bulletPanel);
            DependencyObject dp = (DependencyObject)bulletPanel.InputHitTest(p);
            BulletLine bl = getBulletLine(dp);

            if (bl != null)
            {
                blpos = bl.Priority - 1;
                bl.showTop();
            }
            else blpos = 32768;
            if (data.StartsWith("BPID In ")) e.Handled = true;
        }

        BulletLine getBulletLine(DependencyObject dp)
        {
            if (dp == null) return null;
            DependencyObject p = VisualTreeHelper.GetParent(dp);
            if (p == null || p is BulletLine) return p as BulletLine;
            return getBulletLine(p);
        }

        BulletLine getBulletLineChild(DependencyObject dp)
        {
            if (dp == null) return null;
            BulletLine ret = null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
            {
                DependencyObject p = VisualTreeHelper.GetChild(dp, i);
                if (p == null || p is BulletLine)
                {
                    ret = p as BulletLine;
                    break;
                }
                else ret = getBulletLine(p);
            }
            return ret;
        }

        void bullet_Drop(object sender, DragEventArgs e)
        {
            clearTops();
            string data = e.Data.GetData(typeof(string)).ToString();
            if (data.StartsWith("BPID In ") && EditMode && !Window1.EditTranslation) addBullet(e.Data.GetData(typeof(string)).ToString());
        }

		internal void addBulletToEnd(string p)
		{
			blpos = bu.Count + 1;
			addBullet(p);
		}

        internal void addBullet(string p)
        {
            string[] bp = p.Split('(', ')');
            if (bp.Length >= 4)
            {
                string bulletText = p.Substring(p.IndexOf('(', 10) + 1);
                bulletText = bulletText.Substring(0, bulletText.Length - 1);
                string[] ids = bp[1].Split(',');
                string[] des = bulletText.Replace("$$BREAK$$", ((char)7).ToString()).Split((char)7);
                if (ids.Length <= des.Length)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
						int id;
						int recid;
						if (!int.TryParse(ids[i], out id)) continue;
						if (asset["Table"]["RecID"].InnerText == "new") recid = 0;
						else recid = int.Parse(asset["Table"]["RecID"].InnerText);
                        BulletUse nbu = new BulletUse(recid, id, des[i], blpos);
                        bu.Add(nbu);
                        Bullets.Add(nbu.Bullet);
                    }
                    drawBullets();
                }
            }
        }

        StackPanel bulletPanel;
        GridSplitter gs;
		Button btnBullet;
		Button btnHideBullet;
		countIndicator bulletCount;
		public bool bulletsVisible = false;

        void btnBullet_Click(object sender, RoutedEventArgs e)
        {
			animateBulletPanel();
            if (bulletsVisible) getBullets();
        }

		private void animateBulletPanel()
		{
			RowDefinition cr = (RowDefinition)this.GetTemplateChild("contentRow");
			RowDefinition br = (RowDefinition)this.GetTemplateChild("bulletRow");
			GridSplitter gs1 = (GridSplitter)this.GetTemplateChild("gridSplitter1");
			double crh = cr.Height.Value;
			double brh = br.Height.Value;

			double[] from = { crh, crh, brh, brh };
			double[] to = { 100, 200, 100, 0 };
			string[] btn = { "Hide Bullets", "Show Bullets" };
			int set = bulletsVisible ? 1 : 0;

			GridLengthAnimation cra = new GridLengthAnimation(from[set], to[set], new Duration(TimeSpan.FromSeconds(0.3)), GridUnitType.Star);
			GridLengthAnimation bra = new GridLengthAnimation(from[set + 2], to[set + 2], new Duration(TimeSpan.FromSeconds(0.3)), GridUnitType.Star);
			cra.Completed += delegate(object o, EventArgs ea)
			{
				cr.Height = new GridLength(to[set], GridUnitType.Star);
			};
			bra.Completed += delegate(object o, EventArgs ea)
			{
				br.Height = new GridLength(to[set + 2], GridUnitType.Star);
			};
			cra.FillBehavior = FillBehavior.Stop;
			bra.FillBehavior = FillBehavior.Stop;
			cr.BeginAnimation(RowDefinition.HeightProperty, cra);
			br.BeginAnimation(RowDefinition.HeightProperty, bra);
			if (btnBullet.Visibility == Visibility.Visible)
			{
				btnBullet.Visibility = Visibility.Collapsed;
				btnHideBullet.Visibility = Visibility.Visible;
			}
			else
			{
				btnHideBullet.Visibility = Visibility.Collapsed;
				btnBullet.Visibility = Visibility.Visible;
			}
			bulletsVisible = !bulletsVisible;
			gs1.IsEnabled = bulletsVisible;
		}

        List<BulletUse> bu = new List<BulletUse>();
        XElement Bullets = null;

        private void getBullets()
        {
			loadBullets();
            drawBullets();
        }

		private void loadBullets()
		{
		    if (Bullets != null) return;
			string recid;
			if (asset["Table"]["RecID"].InnerText == "new") recid = "-1024";
			else recid = asset["Table"]["RecID"].InnerText;
			XElement xe = DataAccess.getData("ab_GetAsset", new string[] {
                "@AssetTypeID", "7",
                "@AssetID", recid
            }, false);
			bu.Clear();
			Bullets = xe;
			var bullets = from el in xe.Elements("Table") select el;
			foreach (XElement item in bullets)
			{
				BulletUse b = new BulletUse(item);
				//{
				//    BUID = int.Parse(item.Element("BUID").Value),
				//    BPID = int.Parse(item.Element("BPID").Value),
				//    BP_TEXT = item.Element("BP_TEXT").Value,
				//    Priority = int.Parse(item.Element("Priority").Value),
				//};
				if (Window1.ShowTranslation) b.BP_TEXTLanguage = DataAccess.getLanguage(5, b.BPID, Window1.TranslationLanguage).SelectSingleNode("//BP_TEXT").InnerText;
				bu.Add(b);
			}
		}

        private void drawBullets()
        {
            StackPanel bp = (StackPanel)this.GetTemplateChild("bulletPanel");
            bp.Children.Clear();
            int to = 0;
            foreach (BulletUse item in bu.OrderBy(i => i.Priority))
            {
                DockPanel dp = new DockPanel();
                ToggleButton tb = new ToggleButton();
                tb.Height = 24;
                tb.Width = 24;
                item.toggleButton = tb;
                Image img = new Image();
                img.Source = new BitmapImage(new Uri("images/Play_16x16.png", UriKind.Relative));
                tb.Content = img;
                dp.Children.Add(tb);
                BulletLine l = new BulletLine { ID = item.BPID, Text = item.BP_TEXT, Priority = ++to, TextLanguage = item.BP_TEXTLanguage };
                l.Tag = to;
                item.Priority = to;
                l.GotFocus += new RoutedEventHandler(l_GotFocus);
				l.MouseDoubleClick += new MouseButtonEventHandler(l_MouseDoubleClick);
                item.bulletLine = l;
                dp.Children.Add(l);
                bp.Children.Add(dp);
            }
            maxTag = to;
			if (asset["Table"]["BulletCount"] == null) asset["Table"].AppendChild(asset.OwnerDocument.CreateElement("BulletCount"));
			asset["Table"]["BulletCount"].InnerText = bu.Count.ToString();
        }

		void l_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!EditMode)
			{
				BulletUse bl = SelectedBullet;

				cat.AssetTypeId = 5;
				Window1.RadioToggle(cat.Form.assetGroup, cat.AssetTypeId);
				cat.LoadAssetFromList(bl.BPID.ToString(), "@conclusionid:" + bl.RECID.ToString(), bl.RECID.ToString());
			}
		}

        public override void Cancel()
        {
            base.Cancel();
            if (Bullets != null && bulletsVisible)
            {
                getBullets();
			}
			if (bulletsVisible)
			{
				btnBullet.Visibility = System.Windows.Visibility.Collapsed;
				btnHideBullet.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				btnBullet.Visibility = System.Windows.Visibility.Visible;
				btnHideBullet.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			if (bulletsVisible)
			{
				getBullets();
			}
		}

        public override bool Save(bool toSaas = false)
        {
            cat.Form.clearSubCat();
            var save = base.Save(false);

            if (save && !Window1.EditTranslation)
            {
                if (Bullets != null)// && bulletsVisible)
                {
					if (newID > 0)
						foreach(var item in Bullets.Elements("Table").Where(f => f.Element("RECID").Value == "0"))
							item.Element("RECID").Value = newID.ToString();
                    //MessageBox.Show(Bullets.ToString());
                    XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                        "@xml", Bullets.ToString()
                    }, true);
                }
            }
            if (save)
            {
                reloadAsset(originalAsset.SelectSingleNode("Table/*").InnerText);
                SaveToSaas();
            }
            return save;
        }

        int maxTag = 0;
        BulletUse SelectedBullet = null;

        void l_GotFocus(object sender, RoutedEventArgs e)
        {
            BulletLine tb = sender as BulletLine;
            tb.Background = Brushes.LightCyan;
            foreach (var item in bu)
            {
                if (item.bulletLine == tb)
                    SelectedBullet = item;
                else
                    item.bulletLine.Background = Brushes.White;
            }
            if ((int)tb.Tag != 1 && EditMode) ButtonChildren["btnMoveBulletUp"].IsEnabled = true;
            else ButtonChildren["btnMoveBulletUp"].IsEnabled = false;
            if ((int)tb.Tag != maxTag && EditMode) ButtonChildren["btnMoveBulletDown"].IsEnabled = true;
            else ButtonChildren["btnMoveBulletDown"].IsEnabled = false;
            if (EditMode) ButtonChildren["btnRemoveBullet"].IsEnabled = true;
            else ButtonChildren["btnRemoveBullet"].IsEnabled = false;
        }

        public Conclusion()
        {
        }

        void PriorityChange(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            BulletUse lastItem = null;
            foreach (BulletUse item in bu.OrderBy(i => i.Priority))
            {
                if ((b.Name == "btnMoveBulletUp" && item == SelectedBullet && lastItem != null) ||
                    (b.Name == "btnMoveBulletDown" && lastItem == SelectedBullet))
                {
                    int temp = lastItem.Priority;
                    lastItem.Priority = item.Priority;
                    item.Priority = temp;
                }
                lastItem = item;
            }
            drawBullets();
            l_GotFocus(SelectedBullet.bulletLine, null);
        }

        public Conclusion(XmlNode conclusion)
            : base(conclusion)
        {
            assetType = AssetType.Conclusion;
            tableName = "RECOMMENDATION";
            cats.Add(0, "AgeID");
            cats.Add(1, "RecTypeID");
            cats.Add(2, "TimeID");
            cats.Add(3, "ProviderID");
            expert = conclusion["Table"]["Possible_Condition"];
            string silent = conclusion["Table"]["Silent"].InnerText;
            bool bSilent = false;
            if (!bool.TryParse(silent, out bSilent))
                conclusion["Table"]["Silent"].InnerText = bSilent.ToString();
            string info = conclusion["Table"]["Information"].InnerText;
            bool bInfo = false;
            if (!bool.TryParse(info, out bInfo))
                conclusion["Table"]["Information"].InnerText = bInfo.ToString();
        }

		internal void clearBullets()
		{
			bu.Clear();
			if (bulletsVisible)
			{
				drawBullets();
				btnBullet_Click(null, null);
			}
			btnBullet.Visibility = System.Windows.Visibility.Visible;
			btnHideBullet.Visibility = System.Windows.Visibility.Collapsed;
		}

		internal void cleanBullets()
		{
			loadBullets();
			foreach (var item in bu)
			{
				item.BUID = "new";
				item.RECID = 0;
			}
			if (bu.Count > 0 && !bulletsVisible) animateBulletPanel();
			drawBullets();
		}
	}

    class BulletUse
    {
        public string BUID { get { return Bullet.Element("BUID").Value; } set { Bullet.Element("BUID").Value = value; } }
        public int RECID { get { return int.Parse(Bullet.Element("RECID").Value); } set { Bullet.Element("RECID").Value = value.ToString(); } }
        public int BPID { get { return int.Parse(Bullet.Element("BPID").Value); } set { Bullet.Element("BPID").Value = value.ToString(); } }
        public string BP_TEXT { get { return Bullet.Element("BP_TEXT").Value; } }
        public int Priority { get { return int.Parse(Bullet.Element("Priority").Value); } set { Bullet.Element("Priority").Value = value.ToString(); } }
		public string BP_TEXTLanguage { get; set; }
        public BulletLine bulletLine { get; set; }
        public ToggleButton toggleButton { get; set; }
        public XElement Bullet { get; set; }

        public BulletUse(XElement _Bullet)
        {
            Bullet = _Bullet;
        }

        public BulletUse(int _RecID, int _BPID, string _BP_Text, int priority)
        {
            Bullet = new XElement("Table",
                new XElement("BUID", "new"),
                new XElement("RECID", _RecID),
                new XElement("BPID", _BPID),
                new XElement("BP_TEXT", _BP_Text),
                new XElement("Priority", priority));
        }

        public override string ToString()
        {
            return BP_TEXT;
        }
    }
}
