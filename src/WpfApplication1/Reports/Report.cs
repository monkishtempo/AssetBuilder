using AssetBuilder.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Word = Microsoft.Office.Interop.Word;

namespace AssetBuilder.Reports
{
    class Report
    {
        public static Controls.AlgoLoader AlgoLoader { get; set; }
        public static Action DisableLoader = delegate() { if (AlgoLoader != null) AlgoLoader.disableForm(true); };
        public static Action EnableLoader = delegate() { if (AlgoLoader != null) AlgoLoader.enableForm(); };
        public static Dictionary<string, string[]> prompts = new Dictionary<string, string[]>{
				  { "", new string[] { "string", "Please enter the value for {0}.", "Input required", "{0}" } },
				  { ":checkbox", new string[] { "bool", "Please enter the value for {0}.", "Input required", "{0}|9" } },
				  { "ReportType", new string[] { "string", "Please enter the report name.", "Input required", "Name" } },
				  { "Language~", new string[] { "string", "Please enter the base language.", "Input required", "Language" } },
				  { "date", new string[] { "date", "Please enter the start date and include time if required.\r\nAny parsable format is accepted ISO 8601 would be yyyy-MM-dd HH:mm.", "Input required", "Date" } },
				  { "count", new string[] { "int", "Please enter the minimum number of instances.", "Input required", "Count" } },
                  { "TextAsset", null },
                  { "year", null },
                  { "currentDate", null },
				  { "commentType:", new string[] { "string", "Please select the comment filters you want to include in the report.", "Input required", "Comment Filters|0" } },
				  { "addedColumns:0", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Asset Fields|7" } },
				  { "addedColumns:1", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Asset Fields|1" } },
				  { "addedColumns:2", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Asset Fields|2" } },
				  { "addedColumns:3", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Asset Fields|3" } },
				  { "addedColumns:4", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Asset Fields|4" } },
				  { "addedColumns:5", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Asset Fields|5" } },
				  { "merge:", new string[] { "string", "Please select the asset fields you want to include in the report.", "Input required", "Merge Language|6" } },
		};
        static Dictionary<string, int> AssetTypes = new Dictionary<string, int>{
			{ "titleid", 0 },
			{ "algoid", 1 },
			{ "questionid", 2 },
			{ "ansid", 3 },
			{ "recid", 4 },
            { "bpid", 5 },
            { "textassetid", 12 },
        };

        public static void SetUpParameters(string algos, List<string> prms, XsltArgumentList args, List<string> sqlParams, out bool useTraversalService)
        {
            useTraversalService = true;
            if (prms.Count == 0) return;
            if (prms.Contains("algos") && string.IsNullOrEmpty(algos))
            {
                System.Windows.MessageBox.Show("Please select the algos required for the report or type the algoid in the textbox.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
                return;
            }

            if (prms.Contains("assets") && AlgoLoader.lstAssets.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("Please include any assets required for the report.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
                return;
            }

            if (prms[0] == "Conclusion" && Window1.McKesson_Mode)
            {
                args.AddParam("SilentLabel", "", "Self care");
                args.AddParam("InformationLabel", "", "Watchout condition");
            }

            for (int i = 0; i < prms.Count; i++)
            {
                if (prms[i] != "ReportType" && i == 0) sqlParams.AddRange(new string[] { "@ReportType", prms[0].Replace("_Word", "") });
                else if (prms[i] == "algos") sqlParams.AddRange(new string[] { "@Algos", algos });
                else if (prms[i] == "assets")
                {
                    Dictionary<string, List<string>> fullAssetList;
                    //string conclusions;
                    sqlParams.AddRange(new string[] { "@Assets", AlgoLoader.getAssetXml(out fullAssetList).OuterXml });
                    args.AddParam("assets", "", string.Join(", ", fullAssetList.Select(f => f.Key + (f.Value.Count > 1 ? "s" : "") + " : " + string.Join(", ", f.Value))));
                }
                else if (prms[i] == "assetxml")
                {
                    sqlParams.AddRange(new string[] { "@Assets", prms[++i] });
                }
                else if (prms[i] == "url")
                {
                    useTraversalService = true;
                    prms[0] = "XmlTable";
                }
                else
                {
                    string input = "";
                    string[] inputParams = prompts[""];
                    if (prompts.ContainsKey(prms[i])) inputParams = prompts[prms[i]];
                    else if (prms[i].EndsWith(":checkbox")) inputParams = prompts[":checkbox"];
                    else if (prms[i].EndsWith(":true") || prms[i].EndsWith(":false"))
                    {
                        inputParams = null;
                        input = prms[i].Split(':')[1];
                        prms[i] = prms[i].Split(':')[0];
                    }

                    AssetBuilder.Controls.InputBox ib = null;

                    if (inputParams != null)
                    {
                        AssetBuilder.Controls.InputBoxValidate ibv;
                        Enum.TryParse<AssetBuilder.Controls.InputBoxValidate>(inputParams[0], true, out ibv);

                        ib = new Controls.InputBox(
                            string.Format(inputParams[1], prms[i].Split(':')[0]),
                            string.Format(inputParams[2], prms[i].Split(':')[0]),
                            string.Format(inputParams[3], prms[i].Split(':')[0]),
                            System.Windows.WindowStartupLocation.CenterScreen, ibv);

                        ib.ShowDialog();
                        if (!ib.DialogResult.HasValue || !ib.DialogResult.Value) return;
                        if (inputParams[0] == "date")
                        {
                            DateTime dt;
                            if (!DateTime.TryParse(ib.Text, out dt)) return;
                            input = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else if (inputParams[0] == "int")
                        {
                            int c;
                            if (!int.TryParse(ib.Text, out c)) return;
                            input = c.ToString();
                        }
                        else input = ib.Text;
                        if (prms[i] == "ReportType") input = input.Split('|')[0].Replace("_Word", "");
                        if (!prms[i].Contains(":"))
                        {
                            string sqlprefix = "@";
                            if (prms[i].Contains("~"))
                            {
                                sqlprefix = "";
                                prms[i] = prms[i].Split('~')[0];
                            }
                            sqlParams.AddRange(new string[] { sqlprefix + prms[i], input });
                        }
                        if (prms[i] == "ReportType")
                        {
                            prms[i] = input;
                            if (ib.Text.Contains('|'))
                            {
                                prms.AddRange(ib.Text.Split('|').Skip(1));
                            }
                        }
                        prms[i] = prms[i].Split(':')[0];
                        if (inputParams[0] == "bool" && ib.Text == prms[i]) input = "true";
                    }
                    else if (prms[i] == "year")
                    {
                        input = DateTime.Now.Year.ToString();
                    }
                    else if (prms[i] == "currentDate")
                    {
                        input = DateTime.Now.ToString("MMMM d, yyyy");
                    }
                    else if (prms[i] == "TextAsset")
                    {
                        sqlParams.AddRange(new string[] { prms[i], "Content" });
                        input = "Content";
                    }

                    if (args.GetParam(prms[i], "") != null)
                    {
                        input = args.GetParam(prms[i], "") + input;
                        args.RemoveParam(prms[i], "");
                    }
                    if (!prms[i].Contains("/")) args.AddParam(prms[i], "", input);
                }
            }
        }

        public static void RunReport(string algos, List<string> prms, string excludedAlgos = "", string excludedAlgoList = "", object Tag = null)
        {
            List<string> sqlParams = new List<string>();
            XsltArgumentList args = new XsltArgumentList();
            bool useTraversalService = false;

            try
            {
                SetUpParameters(algos, prms, args, sqlParams, out useTraversalService);
            }
            finally
            {
                if (AlgoLoader != null) AlgoLoader.enableForm();
            }

            if (AlgoLoader != null) AlgoLoader.disableForm(true);
            try
            {
                XElement xn = null;
                if (useTraversalService)
                {
                    // Example "PriorityReport/xml/4843|url";
                    var url = new Uri(new Uri(Settings.Default.WebService), $"TraversalService/TableOutput/{sqlParams[1]}").AbsoluteUri;
                    xn = XElement.Parse(Extension.GetWebRequest(url));
                }
                else
                {
                    xn = DataAccess.getData("ab_Report", sqlParams.ToArray(), false);

                    if (!string.IsNullOrEmpty(excludedAlgos))
                    {
                        Dictionary<string, List<Tuple<string, string>>> mapping = new Dictionary<string, List<Tuple<string, string>>>{
                           {"CheckOld", new List<Tuple<string, string>>(new Tuple<string, string>[] { Tuple.Create("Table2", "Table1"), Tuple.Create("Table4", "Table2"), Tuple.Create("Table6", "Table3"), Tuple.Create("Table8", "Table4") }) },
                           {"AssetReport", new List<Tuple<string, string>>(new Tuple<string, string>[] { Tuple.Create("Table1", "Table1"), Tuple.Create("Table2", "Table2"), Tuple.Create("Table3", "Table3"), Tuple.Create("Table4", "Table4") }) },
                           {"Conclusion", new List<Tuple<string, string>>(new Tuple<string, string>[] { Tuple.Create("Table1", "Table3") }) },
                           {"ConclusionSummary", new List<Tuple<string, string>>(new Tuple<string, string>[] { Tuple.Create("Table1", "Table3") }) },
                           {"QuestionAnswer", new List<Tuple<string, string>>(new Tuple<string, string>[] { Tuple.Create("Table1", "Table1") }) },
                        };

                        XElement exclude = DataAccess.getData("ab_report", new string[] {
                            "@ReportType", "AssetReport",
                            "@Algos", excludedAlgos
                        }, false);

                        args.AddParam("excludedAlgoList", "", excludedAlgoList);

                        if (mapping.ContainsKey(sqlParams[1]))
                        {
                            foreach (var item in mapping[sqlParams[1]])
                            {
                                xn.Elements(item.Item1).Where(f => exclude.Elements(item.Item2).Any(e => e.Elements().First().Value == f.Elements().First().Value)).Remove();
                            }
                        }
                    }
                }
                if (Window1.ShowTranslation)
                {
                    var assets = xn.Elements();
                    foreach (var item in assets)
                    {
                        int id = int.Parse(item.Elements().First().Value);
                        int assettype = AssetTypes[item.Elements().First().Name.LocalName.ToLower()];
                        if (assettype != 5 && item.Element("BPID") != null)
                        {
                            id = int.Parse(item.Element("BPID").Value);
                            assettype = 5;
                            item.Add(DataAccess.getLanguage(assettype, id, Window1.TranslationLanguage).GetXElement());
                        }
                        else if (assettype == 0)
                            item.Add(DataAccess.getLanguage(assettype, item.Elements().Skip(1).First().Value, Window1.TranslationLanguage).GetXElement());
                        else
                            item.Add(DataAccess.getLanguage(assettype, id, Window1.TranslationLanguage).GetXElement());
                        if (assettype != 3 && item.Element("AnswerID") != null)
                        {
                            id = int.Parse(item.Element("AnswerID").Value);
                            assettype = 3;
                            item.Add(DataAccess.getLanguage(assettype, id, Window1.TranslationLanguage).GetXElement());
                        }
                    }
                }

                if (File.Exists($"Reports\\{prms[0]}\\Layout.html"))
                {
                    XmlReportBase xrb;
                    var folder = $"Reports\\{prms[0]}";
                    if (prms[0] == "ConclusionSummary") xrb = new ConclusionSummary() { Folder = folder };
                    else xrb = new XmlReportBase() { Folder = folder };
                    if (Tag != null & Tag is Dictionary<string, string>) xrb.prms = Tag as Dictionary<string, string>;
                    xrb.Completed += delegate (object obj, CompletedEventArgs ea)
                    {
                        if (ea.UniqueID == null) AlgoLoader.ScriptText = ea.Content;
                        else AlgoLoader.ScriptHtml = ea.Content;
                        AlgoLoader.PageID = ea.UniqueID;
                        //output.NavigateToString(content);
                        AlgoLoader.enableForm();
                    };
                    xrb.RunReport(xn, "@Layout@");
                }
                else
                {
                    prms[0] = prms[0].Replace("_Word", "");
                    Functions func = new Functions();
                    args.AddExtensionObject("e24:Functions", func);

                    XDocument report = XDocument.Parse(xn.Transform(string.Format("Reports\\{0}.xsl", prms[0]), args));
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(runXslReport);
                    Thread t = new Thread(ts);
                    t.Start(report);
                }
            }
            catch (Exception ex)
            {
                if (AlgoLoader != null) AlgoLoader.enableForm();
                System.Windows.MessageBox.Show(ex.Message);
                throw ex;
            }
            //firstPage(oTmpDoc, algoNames(xn));
            //firstPage(oTmpDoc2, algoNames(xn));
        }

        public static Dictionary<string, Word.WdOrientation> orientation = new Dictionary<string, Word.WdOrientation> 
		{ 
			{"Landscape", Word.WdOrientation.wdOrientLandscape},
			{"Portrait", Word.WdOrientation.wdOrientPortrait}
		};
        public static Dictionary<string, int> alignment = new Dictionary<string, int> 
		{ 
			{"Left", 0},
			{"Centre", 1},
			{"Right", 2}
		};

        private static void runXslReport(object data)
        {
            Word.Application word = null;
            try
            {
                if (!(data is XDocument)) return;
                XDocument report = data as XDocument;
                word = new Word.Application();
                //word.Visible = true;

                var documents = report.Element("Report").Elements("Document");
                List<Word.Document> docs = new List<Word.Document>();
                foreach (var document in documents)
                {
                    Word.Document oTmpDoc = word.Documents.Add();
                    oTmpDoc.Styles["Normal"].NoSpaceBetweenParagraphsOfSameStyle = true;
                    oTmpDoc.SpellingChecked = false;
                    oTmpDoc.GrammarChecked = false;
                    docs.Add(oTmpDoc);
                    oTmpDoc.Styles.Add("Spelling", 1);
                    if (orientation.ContainsKey(document.AttributeValue("Orientation")))
                        oTmpDoc.PageSetup.Orientation = orientation[document.AttributeValue("Orientation")];
                    if (document.AttributeValue("LeftMargin") != "")
                        oTmpDoc.PageSetup.LeftMargin = float.Parse(document.AttributeValue("LeftMargin"));
                    var content = document.Nodes();
                    foreach (var item in content)
                    {
                        SetContent(getNextRange(oTmpDoc), item);
                    }
                }

                var worldmls = report.Element("Report").Elements("WordML");
                foreach (var wordml in worldmls)
                {
                    Word.Document oTmpDoc = word.Documents.Add();
                    oTmpDoc.Styles["Normal"].NoSpaceBetweenParagraphsOfSameStyle = true;
                    oTmpDoc.SpellingChecked = false;
                    oTmpDoc.GrammarChecked = false;
                    docs.Add(oTmpDoc);
                    oTmpDoc.Styles.Add("Spelling", 1);
                    string wml = oTmpDoc.Range().XML;
                    var reader = wordml.CreateReader();
                    reader.MoveToContent();
                    string ML = reader.ReadInnerXml().Replace(" xmlns:w=\"http://schemas.microsoft.com/office/word/2003/wordml\"", "");
                    oTmpDoc.Range().InsertXML(wml.Replace("<w:body>", "<w:body>" + ML));
                    if (orientation.ContainsKey(wordml.AttributeValue("Orientation")))
                        foreach (Word.Section section in oTmpDoc.Sections)
                            section.PageSetup.Orientation = orientation[wordml.AttributeValue("Orientation")];
                    if (wordml.AttributeValue("LeftMargin") != "")
                        foreach (Word.Section section in oTmpDoc.Sections)
                            section.PageSetup.LeftMargin = float.Parse(wordml.AttributeValue("LeftMargin"));
                    if (wordml.AttributeValue("RightMargin") != "")
                        foreach (Word.Section section in oTmpDoc.Sections)
                            section.PageSetup.RightMargin = float.Parse(wordml.AttributeValue("LeftMargin"));
                    if (wordml.AttributeValue("PageSize") != "")
                        foreach (Word.Section section in oTmpDoc.Sections)
                            section.PageSetup.PaperSize = (Word.WdPaperSize)Enum.Parse(typeof(Word.WdPaperSize), "wdPaper" + wordml.AttributeValue("PageSize"));
                    if (oTmpDoc.Sections.Count > 1 && wordml.AttributeValue("DeleteLastSection") == "True")
                    {
                        clearheaders(oTmpDoc.Sections.Last);
                        clearcontent(oTmpDoc.Sections.Last);
                        oTmpDoc.Application.Selection.HomeKey(Word.WdUnits.wdStory);
                    }
                }

                var compares = report.Element("Report").Elements("Compare");
                foreach (var compare in compares)
                {
                    int original = 0;
                    int revised = 0;
                    if (int.TryParse(compare.AttributeValue("Original"), out original)
                        && int.TryParse(compare.AttributeValue("Revised"), out revised))
                    {
                        word.CompareDocuments(docs[original - 1], docs[revised - 1]);
                        ((Word._Document)docs[original - 1]).Close(SaveChanges: false);
                        ((Word._Document)docs[revised - 1]).Close(SaveChanges: false);
                    }
                }
            }
            catch (Exception ex)
            {
                App.WriteError(ex);
                System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (AlgoLoader != null) AlgoLoader.Dispatcher.Invoke(EnableLoader, System.Windows.Threading.DispatcherPriority.Render, null);
                if (word != null)
                {
                    word.Visible = true;
                    word.Activate();
                }
            }
        }

        private static void clearcontent(Word.Section section)
        {
            section.Range.Delete();
            section.Range.Select();
            section.Application.Selection.MoveEnd();
            section.Application.Selection.Delete(Word.WdUnits.wdCharacter, 1);
            section.Application.Selection.TypeBackspace();
            section.Application.Selection.Delete(Word.WdUnits.wdCharacter, 1);            
        }

        private static void clearheaders(Word.Section section)
        {

        }

        private static void SetContent(Word.Range oRange, XNode item)
        {
            string name = "";
            XElement elem = null;
            if (item is XElement)
            {
                elem = item as XElement;
                name = elem.Name.ToString();
            }

            switch (name)
            {
                case "PageBreak":
                    oRange.InsertBreak();
                    break;
                case "Content":
                    SetContent(oRange, item, false);
                    break;
                case "BulletList":
                    foreach (var lItem in elem.Nodes())
                    {
                        SetContent(oRange, lItem, true);
                    }
                    break;
                case "Table":
                    List<List<List<XElement>>> data = new List<List<List<XElement>>>();
                    List<float> widths = new List<float>();
                    foreach (var row in elem.Elements("Row"))
                    {
                        List<List<XElement>> rowContent = new List<List<XElement>>();
                        data.Add(rowContent);
                        foreach (var cell in row.Elements("Cell"))
                        {
                            if (cell.AttributeValue("Width") != "") widths.Add(float.Parse(cell.AttributeValue("Width")));
                            List<XElement> cellContent = new List<XElement>();
                            rowContent.Add(cellContent);
                            foreach (var content in cell.Elements())
                            {
                                cellContent.Add(content);
                            }
                        }
                    }
                    InsertTable(oRange, data, widths.ToArray(), item);
                    break;
                case "LineBreak":
                    InsertLineBreak(oRange);
                    break;
                default:
                    SetContent(oRange, item, false);
                    break;
            }
        }

        private static void SetContent(Word.Range oRange, XNode item, bool IsList)
        {
            CellContent cc = new CellContent(item);
            InsertText(oRange, cc, IsList: IsList);
        }

        private static Word.Range getNextRange(Word.Document oTmpDoc)
        {
            Word.Range oRange = oTmpDoc.Range();
            return getNextRange(oRange);
        }

        private static Word.Range getNextRange(Word.Range oRange)
        {
            oRange.EndOf();
            return oRange;
        }

        private static void InsertTable(Word.Range oRange, List<List<List<XElement>>> content, float[] widths, XNode elem)
        {
            if (content == null || content.Count == 0 || content[0] == null || content[0].Count == 0) return;
            int NumRows = content.Count;
            int NumColumns = content[0].Count;
            oRange = getNextRange(oRange);
            Word.Table table = oRange.Tables.Add(oRange, NumRows, NumColumns);
            if (elem.AttributeValue("Indent") != "") table.Rows.LeftIndent = float.Parse(elem.AttributeValue("Indent"));
            for (int y = 0; y < NumRows; y++)
            {
                for (int x = 0; x < NumColumns; x++)
                {
                    if (content[y] != null && content[y].Count > x)
                    {
                        bool first = true;
                        foreach (var item in content[y][x])
                        {
                            if (first)
                                oRange = table.Cell(y + 1, x + 1).Range;
                            else
                                oRange.EndOf(Word.WdUnits.wdCell, 0);
                            if (!string.IsNullOrEmpty(item.Value)) SetContent(oRange, item);
                            first = false;
                        }
                    }
                }
            }
            FormatTable(widths, NumColumns, table);
        }

        private static void FormatTable(float[] widths, int NumColumns, Word.Table table)
        {
            float w = table.Columns[1].Width * NumColumns;
            for (int x = 0; x < widths.Length; x++)
            {
                table.Columns[x + 1].Width = w * widths[x];
            }
            table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
        }

        private static void InsertBreak(Word.Document oTmpDoc)
        {
            Word.Range oRange = getNextRange(oTmpDoc);
            oRange.InsertBreak();
        }

        private static void InsertLineBreak(Word.Range oRange)
        {
            oRange.InsertBreak(Word.WdBreakType.wdLineBreak);
        }

        private static void InsertText(Word.Range oRange, CellContent content,
            bool IsList = false)
        {
            oRange.InsertAfter(content.Text);
            oRange.ParagraphFormat.Alignment = (Word.WdParagraphAlignment)content.Align;
            oRange.Bold = content.Bold ? 1 : 0;
            oRange.Italic = content.Italic ? 1 : 0;
            if (content.FontSize > 0) oRange.Font.Size = content.FontSize;
            if (IsList) oRange.ListFormat.ApplyBulletDefault();
            oRange.ParagraphFormat.LeftIndent = content.Indent;
            if (content.Border) setBorders(oRange);
        }

        static void setBorders(Word.Range oRange)
        {
            oRange.Borders[(Word.WdBorderType)1].LineStyle = Word.WdLineStyle.wdLineStyleSingle;
            oRange.Borders[(Word.WdBorderType)1].LineWidth = Word.WdLineWidth.wdLineWidth100pt;
            oRange.Borders[(Word.WdBorderType)1].Color = Word.WdColor.wdColorBlue;
        }
    }

    class CellContent
    {
        public string Text { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public float FontSize { get; set; }
        public int Align { get; set; }
        public bool Border { get; set; }
        public int Indent { get; set; }

        public CellContent()
        {

        }

        public CellContent(string text)
        {
            Text = text;
        }

        public CellContent(XNode item)
        {
            if (item is XElement)
                Text = (item as XElement).Value;
            else if (item is XText)
                Text = (item as XText).Value;
            if (item.AttributeValue("FontWeight") == "Bold") Bold = true;
            if (item.AttributeValue("FontSize") != "") FontSize = float.Parse(item.AttributeValue("FontSize"));
            if (item.AttributeValue("FontStlye") == "Italic") Italic = true;
            if (item.AttributeValue("Align") != "") Align = Report.alignment[item.AttributeValue("Align")];
            if (item.AttributeValue("Border") != "") Border = bool.Parse(item.AttributeValue("Border"));
            if (item.AttributeValue("Indent") != "") Indent = int.Parse(item.AttributeValue("Indent"));
            if (item.AttributeValue("Break") == "") Text += Environment.NewLine;
            else for (int i = 0; i < int.Parse(item.AttributeValue("Break")); i++) Text += Environment.NewLine;
        }

        public static implicit operator CellContent(string text)
        {
            return new CellContent(text);
        }

        public static implicit operator string(CellContent cc)
        {
            return cc.Text;
        }
    }

    class Functions
    {
        public List<int> conc = new List<int>();

        public XPathNavigator Parse(string data)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);
            return doc.CreateNavigator();
        }

        public int max(XPathNodeIterator nodeset, string RiskFactor)
        {
            int i = int.MinValue;

            while (nodeset.MoveNext())
            {
                int o;
                if (nodeset.Current.SelectSingleNode("Factor").Value == RiskFactor &&
                    int.TryParse(nodeset.Current.SelectSingleNode("ChangeID").Value, out o))
                {
                    int ConclusionID = int.Parse(nodeset.Current.SelectSingleNode("ConclusionID").Value);
                    if (conc.Contains(ConclusionID) && o > i) i = o;
                }
            }

            return i;
        }

        public int Includes(XPathNodeIterator nodeset, int ConclusionID)
        {
            while (nodeset.MoveNext())
            {
                if (nodeset.Current.SelectSingleNode("Includes") != null && nodeset.Current.SelectSingleNode("Includes").Value == ConclusionID.ToString())
                    return int.Parse(nodeset.Current.SelectSingleNode("ConclusionID").Value);
            }
            return ConclusionID;
        }

        public bool newer(XPathNodeIterator nodeset, string date)
        {
            while (nodeset.MoveNext())
            {
                DateTime dt = DateTime.Parse(nodeset.Current.Value);
                return dt > DateTime.Parse(date);
            }
            return false;
        }

        public bool ContainsAny(XPathNodeIterator nodeset, string filter)
        {
            while (nodeset.MoveNext())
            {
                string node = nodeset.Current.Value;
                string[] find = filter.Split('|');
                bool ca = find.Any(f => (f == "" && node == "") || (f != "" && node.Contains(f)));
                return ca;
            }
            return false;
        }
    }
}
