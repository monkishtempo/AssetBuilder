using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Xsl;
using AssetBuilder.Properties;
using System.Threading.Tasks;

namespace AssetBuilder
{
    static class DataAccess
    {
        static internal string LastProcedure = "";
        static internal string[] LastParameters = new string[0];
        static internal List<Tuple<string, XmlNode, JNode, TimeSpan>> LastCommands = new List<Tuple<string, XmlNode, JNode, TimeSpan>>(10);
        static internal string LastCommand;
        static internal Dictionary<string, Data.Data> dataServices = new Dictionary<string, Data.Data>();
        static object lockObj = new object();
        private static Dictionary<string, List<Classes.ABEnvironment>> _Environments;
        public static Dictionary<string, List<Classes.ABEnvironment>> Environments
        {
            get
            {
                if (_Environments == null) _Environments = GetEnvironments();
                return _Environments;
            }
        }

        public static void ClearEnvironments()
        {
            _Environments = null;
        }

        private static Dictionary<string, List<Classes.ABEnvironment>> GetEnvironments()
        {
            var defaults = qcat.BuilderDefaults;//getDataNode("ab_builderdefaults", null, false);
            return 
                defaults.SelectNodes("//*/EnvironmentName")
                    .OfType<XmlNode>()
                    .Select(f => f.InnerText)
                    .Distinct()
                    .ToDictionary(f => f, g => defaults.SelectNodes("//*[EnvironmentName = '" + g + "']").OfType<XmlNode>().Select(n => new Classes.ABEnvironment(n["EnvironmentName"].InnerText, n["EnvironmentType"].InnerText, n["EnvironmentUrl"].InnerText, n["SSLAlso"].InnerText, n["Encrypted"].InnerText)).ToList());
        }

        public static Dictionary<string, string> WebBuilders
        {
            get
            {
                return Environments.SelectMany(f => f.Value).Where(f => f.Type == "WebBuilder")
                    .ToDictionary(k => k.Name, v => v.Url);
            }
        }

        public static string JsonSerialize(object obj)
        {
            var js = new JavaScriptSerializer();
            var json = js.Serialize(obj);
            return json;
        }

        public static Dictionary<string, object> JsonDeSerialize(string json)
        {
            var js = new JavaScriptSerializer();
            var obj = js.Deserialize<Dictionary<string, object>>(json);
            return obj ?? new Dictionary<string, object>();
        }

        public static T JsonDeSerialize<T>(string json) where T : class, new()
        {
            if (json == null) return new T();
            var js = new JavaScriptSerializer();
            try
            {
                var obj = js.Deserialize<T>(json);
                return obj ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        public static Data.Data GetDataService(string url = null)
        {
            if (url == null) url = Settings.Default.WebService;
            lock (lockObj)
            {
                if (!dataServices.ContainsKey(url))
                {
                    var ds = new AssetBuilder.Data.Data
                    {
                        Url = url,
                        Proxy = System.Net.WebRequest.DefaultWebProxy,
                        Credentials = System.Net.CredentialCache.DefaultCredentials
                    };
                    ds.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    dataServices.Add(url, ds);
                    ServicePointManager.UseNagleAlgorithm = true;
                    ServicePointManager.Expect100Continue = true;
                    //ServicePointManager.CheckCertificateRevocationList = true;
                    //ServicePointManager.DefaultConnectionLimit = ServicePointManager.DefaultNonPersistentConnectionLimit;
                }
            }
            return dataServices[url];
        }

        public static string Domain
        {
            get
            {
                Uri u = new Uri(Settings.Default.WebService);
                return u.GetLeftPart(UriPartial.Authority);
            }
        }

        static internal XmlNode SetProperty(string type, string id, string name, string value)
        {
            var ret = getDataNode("dsp_SetProperty", new string[] { "@PropertyType", type, "@DataID", id, "@PropertyName", name, "@PropertyValue", value }, true);
            var test = type.Split(':')[0];
            if (Window1.AllowSaaSIntegration && test.In("Algo", "Transfer", "Question", "Answer", "Conclusion", "Bullet", "ConclusionMap")) Task.Run(() => SaaS.Instance.SavePropertyToSaas(type, id, name, value));
            return ret;
        }

        static internal XElement getData(string procedure, params string[] parameters)
        {
            return getDataNode(procedure, parameters, true).GetXElement();
        }

        static internal XElement getData(string procedure, string[] parameters, bool withUser, string remoteUrl = null)
        {
            return getDataNode(procedure, parameters, withUser, remoteUrl).GetXElement();
        }

        static internal XmlNode getDataNode(string procedure, params string[] parameters)
        {
            return getDataNode(procedure, parameters, true);
        }

        static internal XmlNode getDataNode(string procedure, string[] parameters, bool withUser, string remoteUrl = null)
        {
            if (parameters == null) parameters = new string[0];
            string last = GenerateCommand(remoteUrl ?? Settings.Default.WebService, procedure, parameters);
            if (withUser && !string.IsNullOrEmpty(Window1.UserName) && !string.IsNullOrEmpty(Window1.Password))
            {
                List<string> prms = new List<string>();
                if (parameters != null) prms.AddRange(parameters);
                if (!prms.Contains("UserName")) prms.AddRange(new string[] { "UserName", Window1.UserName });
                if (!prms.Contains("Password")) prms.AddRange(new string[] { "Password", Window1.Password });
                parameters = prms.ToArray();
            }
            Data.Data ds = GetDataService(remoteUrl);
            LastCommand = last;
            DateTime then = DateTime.Now;
            //string xml = SOAPCall(remoteUrl ?? Settings.Default.WebService, "getData",
            //            (new string[] { "procedure" }).Concat(parameters.Select(f => "args.string")).ToArray(),
            //            (new string[] { procedure }).Concat(parameters).ToArray());
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(xml);
            //XmlNode ret = doc.SelectSingleNode("*/*/*/*/*");
            //ds.getData(procedure, parameters);
            XmlNode ret = ds.getData(procedure, parameters);
            var t = DateTime.Now - then;
            AddLastCommand(last, ret, t);
            return ret;
        }

        internal static XmlNode TableUpdate(string table, string data)
        {
            Data.Data ds = GetDataService();
            return ds.TableUpdate(table, data);
        }

        internal static void AddLastCommand(string lastcommand, XmlNode ret, TimeSpan duration)
        {
            AddLastCommand(lastcommand, ret, null, duration);
        }

        internal static void AddLastCommand(string lastcommand, JNode ret, TimeSpan duration)
        {
            AddLastCommand(lastcommand, null, ret, duration);
        }

        private static void AddLastCommand(string lastcommand, XmlNode ret, JNode json, TimeSpan duration)
        {
            if (LastCommands.Count >= LastCommands.Capacity) LastCommands.RemoveAt(0);
            LastCommands.Add(Tuple.Create(lastcommand, ret, json, duration));
            if (DebugOutput.DebugOuputForm != null)
            {
                if(json != null) DebugOutput.DebugOuputForm.trace.WriteLine(lastcommand, json, duration);
                else DebugOutput.DebugOuputForm.trace.WriteLine(lastcommand, ret, duration);
            }
        }

        public static XmlNode getLanguage(XmlNode asset, string Language)
        {
            if (asset == null)
            {
                XmlNode checklanguage = getLanguage(-1, 0, "");
                Window1.MultiTextLanguage = checklanguage != null && checklanguage.SelectSingleNode("Table[@MultiText='True']") != null;
                return checklanguage;
            }
            int AssetType = getAssetType(asset);
            if (AssetType < 0) return null;
            int id = 0;
            if (int.TryParse(asset["Table"].FirstChild.InnerText, out id))
                return getLanguage(AssetType, id, Language);
            else if (AssetType == 0) return getLanguage(AssetType, asset["Table"]["Title"].InnerText, Language);
            return null;
        }

        public static XmlNode getLanguage(int AssetType, int id, string Language)
        {
            return getLanguage(AssetType, id.ToString(), Language);
        }

        public static XmlNode getLanguage(int AssetType, string text, string Language)
        {
            //Data.Data ds = getDataService();
            DateTime then = DateTime.Now;
            XmlNode ret = callXmlMethod("getLanguage",
                new string[] { "AssetType", "AssetID", "Language" },
                new string[] { AssetType.ToString(), text, Language });
            AddLastCommand(string.Format("GetLanguage({0}, {1}, \"{2}\")", AssetType, text, Language), ret, DateTime.Now - then);
            return ret;
        }

        public static bool categoryLookup = true;

        [Obsolete]
        public static string getCategoryLanguage(int AssetType, string id, string Language)
        {
            if (!categoryLookup) return null;
            //Data.Data ds = DataAccess.getDataService();
            XmlNode res = null;
            try
            {
                DateTime then = DateTime.Now;
                //res = ds.getLanguage(AssetType, id, Language).FirstChild;
                res = callXmlMethod("getLanguage",
                    new string[] { "AssetType", "AssetID", "Language" },
                    new string[] { AssetType.ToString(), id, Language });
                AddLastCommand(string.Format("GetLanguage({0}, {1}, \"{2}\")", AssetType, id, Language), res, DateTime.Now - then);
            }
            catch { categoryLookup = false; }
            return res == null ? null : res.InnerText;
        }

        public static XmlNode setLanguage(XmlNode asset, XmlNode languageAsset, string Language)
        {
            if (asset == null) return null;
            int AssetType = getAssetType(asset);
            return setLanguage(AssetType, asset["Table"].FirstChild.InnerText, languageAsset, Language);
        }

        public static XmlNode setLanguage(int AssetType, string content, XmlNode languageAsset, string Language)
        {
            if (AssetType < 0) return null;
            Data.Data ds = DataAccess.GetDataService();
            DateTime then = DateTime.Now;
            XmlNode ret = ds.setLanguage(AssetType, content, Language, languageAsset);
            //XmlNode ret = callXmlMethod("setLanguage",
            //    new string[] { "AssetType", "AssetID", "Language", "Content" },
            //    new string[] { AssetType.ToString(), content, Language, languageAsset.OuterXml });
            AddLastCommand(string.Format("SetLanguage({0}, {1}, \"{2}\", \"{3}\")", AssetType, content, languageAsset.OuterXml, Language), ret, DateTime.Now - then);
            return ret;
        }

        public static XmlNode searchLanguage(int AssetType, int SearchType, string Language, string search)
        {
            //Data.Data ds = DataAccess.getDataService();
            DateTime then = DateTime.Now;
            //XmlNode ret = ds.searchLanguage(AssetType, SearchType, Language, search);
            XmlNode ret = callXmlMethod("searchLanguage",
                new string[] { "AssetType", "SearchType", "Language", "search" },
                new string[] { AssetType.ToString(), SearchType.ToString(), Language, search });
            AddLastCommand(string.Format("SearchLanguage({0}, {1}, \"{2}\", \"{3}\")", AssetType, SearchType, Language, search), ret, DateTime.Now - then);
            return ret;
        }

        public static int getAssetType(XmlNode asset)
        {
            int AssetType = -1;
            string idName = asset["Table"].FirstChild.Name;
            switch (idName)
            {
                case "Title":
                    AssetType = 0;
                    break;
                case "AlgoID":
                    AssetType = 1;
                    break;
                case "QuestionID":
                    AssetType = 2;
                    break;
                case "AnsID":
                    AssetType = 3;
                    break;
                case "RecID":
                    AssetType = 4;
                    break;
                case "BPID":
                    AssetType = 5;
                    break;
                case "MapID":
                    AssetType = 11;
                    break;
                case "GroupID":
                    AssetType = 13;
                    break;
                default:
                    break;
            }
            return AssetType;
        }

        public static XElement GetXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        public static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }

        public static XmlAttribute AddAttribute(this XmlElement element, string attributeName, string attributeValue)
        {
            XmlAttribute xa = element.OwnerDocument.CreateAttribute(attributeName);
            xa.Value = attributeValue;
            element.Attributes.Append(xa);
            return xa;
        }

        public static XmlElement AddElement(this XmlElement element, string elementName, string elementValue)
        {
            XmlElement xe = element.OwnerDocument.CreateElement(elementName);
            xe.InnerText = elementValue;
            element.AppendChild(xe);
            return xe;
        }

        private static string GenerateCommand(string remoteurl, string procedure, string[] parameters)
        {
            var output = $"{remoteurl}/getData?procedure={procedure}";
            foreach (var item in parameters)
            {
                output += $"&args={item}";
            }
            return output;
        }

        private static string GenerateCommand(string procedure, string[] parameters)
        {
            LastProcedure = procedure;
            LastParameters = parameters;
            string cmd = "";
            cmd += "Exec " + procedure + " ";
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length - 1; i += 2)
                {
                    if (i > 0) cmd += ", ";
                    if (string.Compare(parameters[i], "Password", true) == 0)
                        cmd += string.Format(" {0} = '********'", parameters[i]);
                    else if (parameters[i + 1] == "null")
                        cmd += string.Format("{0} = null", parameters[i]);
                    else
                        cmd += string.Format("{0} = '{1}'", parameters[i], parameters[i + 1]);
                }
            }
            //System.Diagnostics.Debug.WriteLine(cmd);
            return cmd;
        }

        static Dictionary<string, XslCompiledTransform> loadedTransformations = new Dictionary<string, XslCompiledTransform>();
        static Dictionary<string, DateTime> loadedTransformationDates = new Dictionary<string, DateTime>();
        public static string Transform(this XmlNode doc, string xslPath, XsltArgumentList args)
        {
            // set the args
            if (args == null) args = new XsltArgumentList();

            //load the document
            XslCompiledTransform t;
            if (loadedTransformations.ContainsKey(xslPath))
            {
                FileInfo fi = new FileInfo(xslPath);
                if (fi.LastWriteTime != loadedTransformationDates[xslPath])
                {
                    loadedTransformations[xslPath].Load(xslPath);
                    loadedTransformationDates[xslPath] = fi.LastWriteTime;
                }
                t = loadedTransformations[xslPath];
            }
            else
            {
                t = new XslCompiledTransform();
                t.Load(xslPath);
                loadedTransformations.Add(xslPath, t);
                loadedTransformationDates[xslPath] = DateTime.Now;
            }

            StringBuilder sb = new StringBuilder();
            t.Transform(new XmlNodeReader(doc), args, XmlWriter.Create(sb));
            return sb.ToString();
        }

        public static string Transform(this XElement doc, string xslPath, XsltArgumentList args)
        {
            return Transform(doc.GetXmlNode(), xslPath, args);
        }

        public static string AttributeValue(this XNode Node, string AttributeName)
        {
            if (Node is XElement)
            {
                XElement Element = Node as XElement;
                return Element.Attribute(AttributeName) == null ? "" : Element.Attribute(AttributeName).Value;
            }
            else return "";
        }

        public static XmlNode callXmlMethod(string method, string[] inputs, string[] values)
        {
            XmlDocument doc = new XmlDocument();
            string ret = SOAPCall(Settings.Default.WebService, method, inputs, values);
            doc.PreserveWhitespace = true;
            doc.LoadXml(ret);
            return doc.SelectSingleNode("*/*/*/*/*");
        }

        static string SOAPCall(string endPoint, string method, string[] inputs, string[] values)
        {
            string nameSpace = "http://tempuri.org/";
            string SOAPAction = nameSpace + method;
            string resultDetail = string.Empty;

            string input = "";
            string array = null;

            for (int i = 0; i < inputs.Length; i++)
            {
                string item = inputs[i];
                if (array != null && !item.StartsWith(array))
                {
                    input += string.Format("</{0}?", array);
                    array = null;
                }
                if (item.Contains("."))
                {
                    string[] bl = item.Split('.');
                    item = bl[1];
                    if (array != bl[0])
                    {
                        array = bl[0];
                        input += string.Format("<{0}>", array);
                    }
                }
                input += string.Format("<{0}>{1}</{0}>", item, HttpUtility.HtmlEncode(values[i]));
            }

            if (array != null) input += string.Format("</{0}>", array);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(endPoint);

            string formatString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
	<{0} xmlns=""{1}"">
	  {2}
	</{0}>
  </soap:Body>
</soap:Envelope>";

            string postData = String.Format(formatString, method, nameSpace, input);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(postData);
            //Clipboard.SetText(postData);

            //postData.Length
            //postData = postData.Replace("’", "'");
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 2.0.50727.3053)";
            request.Headers.Add("SOAPAction", SOAPAction);
            request.ContentType = "text/xml; charset=utf-8";
            request.ContentLength = System.Text.Encoding.UTF8.GetByteCount(postData);
            //request.ContentLength = postData.Length;

            string output = "";

            lock (lockObj)
            {
                using (StreamWriter post = new StreamWriter(request.GetRequestStream()))
                {
                    post.Write(postData);
                    post.Close();
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        output = reader.ReadToEnd();
                        reader.Close();
                    }
                    response.Close();
                }
            }
            //Clipboard.SetText(output);
            return output;
        }
    }
}
