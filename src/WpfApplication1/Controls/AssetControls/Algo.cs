using System;
using System.Windows;
using System.Xml;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Linq;

namespace AssetBuilder.AssetControls
{
    #region Notes
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AssetBuilder.AssetControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AssetBuilder.AssetControls;assembly=AssetBuilder.AssetControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:Question/>
    ///
    /// </summary>
    #endregion

    public class Algo : assetControl
    {
        static Algo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Algo), new FrameworkPropertyMetadata(typeof(Algo)));
        }

        public Algo()
        {
        }

		Label KeywordLabel;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			KeywordLabel = GetTemplateChild("KeywordLabel") as Label;
			KeywordLabel.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(KeywordLabel_MouseDoubleClick);

		}

		void KeywordLabel_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!EditMode) return;
			XmlDocument doc = cat.getAssetXml("conclusionlist", tableName);
			XElement xe = DataAccess.getData("ab_updateAsset", new string[] {
					"@xml", doc.OuterXml
			}, false);
			if (xe.Element("Table") != null && xe.Element("Table").Element("ConclusionList") != null)
			{
				TextBox t = TextChildren["txtKeywords"];
				t.Text = xe.Element("Table").Element("ConclusionList").Value;
				t.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			}
		}

        public override bool Save(bool toSaas = true)
        {
            if (!Window1.McKesson_Mode) return base.Save();

            var keywords = asset["Table"]["Keywords"].InnerText.ToLower().Split(',').Select(f => f.Trim());
            if (keywords.Any(f => f.Length > 30))
            {
                Window1.setStatus(string.Format("Error: Keyword to long ({0})", keywords.First(f => f.Length > 30)));
                return false;
            }
            string keywordtext = string.Join(",", keywords);
            asset["Table"]["Keywords"].InnerText = keywordtext;

            string originalkeywords = originalAsset["Table"]["Keywords"].InnerText;
            bool save = base.Save();
            if (save && !Window1.EditTranslation && asset["Table"]["Keywords"].InnerText != originalkeywords)
            {
                XElement KeyWordXml = DataAccess.getData("ab_TableEdit", new string[] { "@TableName", "Keywords", "@xml", "<root command=\"get\"/>" });
                var kw = KeyWordXml.Elements().Select(f => new { KeywordID = int.Parse(f.Element("value_KeywordID").Value), Keyword = f.Element("value_Keyword").Value });
                if (kw.Any())
                {
                    int maxid = kw.Max(f => f.KeywordID);
                    var newkeywords = keywordtext.Split(',').Where(f => !kw.Any(k => k.Keyword == f));
                    XElement doc = XElement.Parse("<root command=\"edit\" />");
                    foreach (var item in newkeywords)
                    {
                        maxid += 10;
                        doc.Add(new XElement("Table", new XElement("New"), new XElement("value_KeywordID", maxid), new XElement("value_Keyword", item)));
                    }
                    if (doc.HasElements) DataAccess.getData("ab_TableEdit", new string[] { "@TableName", "Keywords", "@xml", doc.ToString() });
                }
            }
            return save;
        }

        public Algo(XmlNode algo) : base(algo)
        {
            assetType = AssetType.Algo;
            tableName = "ALGO_START";
            expert = algo["Table"]["Algo_Name"];
            string dlr = algo["Table"]["Date_Last_Reviewed"].InnerText;
			DateTime blval; 
			if (DateTime.TryParse(dlr, out blval))
				asset["Table"]["Date_Last_Reviewed"].InnerText = blval.ToString("yyyy-MM-dd HH:mm:ss.fff");

			//if (dlr.Contains("+"))
			//    algo["Table"]["Date_Last_Reviewed"].InnerText = dlr.Substring(0, dlr.IndexOf('+'));
            cats.Add(0, "AgeID");
            cats.Add(1, "CatID");
            cats.Add(2, "SubCatID");
            cats.Add(3, "Cat2ID");
        }
    }
}
