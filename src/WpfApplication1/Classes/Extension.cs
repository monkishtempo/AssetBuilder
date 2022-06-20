using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml;
using Point = System.Windows.Point;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AssetBuilder
{
    static class Extension
    {
        public static string PropertyReplace(this string s, object obj)
        {
            return Regex.Replace(s, "@(.*?)@", mi =>
            {
                if (obj == null) return "";
                var group = mi.Groups[1].Value.Split('?');
                var condition = group[0].Split('=');
                var prop = condition[0];
                var template = group.Length > 1 ? group[1] : null;
                var p = obj.GetType().GetProperty(prop);
                var value = p?.GetValue(obj, null).ToString();

                if (condition.Length > 1 && group.Length > 1)
                {
                    var c = group[1].Split(':');
                    var t = c[0];
                    var f = c.Length > 1 ? c[1] : "";
                    if (string.Compare(value, condition[1], true) == 0) value = t;
                    else value = f;
                }

                return value ?? "";
            });
        }

        public static async Task<string> GetContent(this string Url)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                return await wc.DownloadStringTaskAsync(Url);
            }
        }

        public static T GetContent<T>(this string url, (string key, string value)[] headers = null) where T : class
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                using (var wc = new WebClient())
                {
                    if (headers?.Length > 0)
                    {
                        foreach (var header in headers)
                        {
                            wc.Headers.Add(header.key, header.value);
                        }
                    }
                    var b = wc.DownloadString(url);
                    if (string.IsNullOrWhiteSpace(b)) return null;
                    return typeof(T).FullName == "System.String" ? b as T :
                        (typeof(T).FullName == "AssetBuilder.JNode" ? JNode.CreateFromJson(b) as T : js.Deserialize<T>(b));
                }
            }
            catch (WebException ex)
            {
                var we = ((System.Net.HttpWebResponse)ex.Response);
                var output = "";
                using (var reader = new StreamReader(we.GetResponseStream()))
                {
                    output = reader.ReadToEnd();
                    reader.Close();
                }
                return typeof(T).FullName == "System.String" ? output as T :
                    (typeof(T).FullName == "AssetBuilder.JNode" ? JNode.FromObject(new { error = ex.Message, status = we.StatusCode }) as T : null);
            }
        }

        public static T PostObject<T>(this object body, string Url, (string key, string value)[] headers = null, string method = "POST") where T : class
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                using (var wc = new WebClient())
                {
                    if (headers?.Length > 0)
                        foreach (var header in headers)
                            wc.Headers.Add(header.key, header.value);

                    var b = wc.UploadString(Url, method, (body is string) ? body.ToString() : js.Serialize(body));
                    if (string.IsNullOrWhiteSpace(b)) return null;
                    return typeof(T).FullName == "System.String" ? b as T :
                        (typeof(T).FullName == "AssetBuilder.JNode" ? JNode.CreateFromJson(b) as T : js.Deserialize<T>(b));
                }
            }
            catch(WebException ex)
            {
                var we = ((System.Net.HttpWebResponse)ex.Response);
                var output = "";
                using (var reader = new StreamReader(we.GetResponseStream()))
                {
                    output = reader.ReadToEnd();
                    reader.Close();
                }
                return typeof(T).FullName == "System.String" ? output as T :
                    (typeof(T).FullName == "AssetBuilder.JNode" ? JNode.FromObject(new { error = ex.Message, status = we.StatusCode }) as T : null);
            }
        }

        public static async Task<string> PostSoapContent(this string Url, string method, string[] inputs, string[] values)
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
                    input += string.Format("</{0}>", array);
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
                input += string.Format("<{0}>{1}</{0}>", item, values[i]);
            }

            if (array != null) input += string.Format("</{0}>", array);

            string formatString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
	<{0} xmlns=""{1}"">
	  {2}
	</{0}>
  </soap:Body>
</soap:Envelope>";
            string postData = String.Format(formatString, method, nameSpace, input);
            using (var wc = new WebClient())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers.Add("Content-Type", "text/xml; charset=utf-8");
                wc.Headers.Add("SOAPAction", SOAPAction);
                return await wc.UploadStringTaskAsync(Url, postData);
            }
        }

        public static string GetWebRequest(string url, Dictionary<string, string> headers = null)
        {
            string sResponse = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            wc.Headers.Add(item.Key, item.Value);
                        }
                    }
                    sResponse = wc.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                App.WriteError(ex);
            }
            return sResponse;
        }
        public static bool IsGenericList(this object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public static IEnumerator<T> GetGenericEnumerator<T>(this IList<T> o)
        {
            return o.GetEnumerator();
        }

        static readonly Regex b64 = new Regex(@"^[a-zA-Z0-9\+\/]*={0,2}$", RegexOptions.Compiled);
        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && b64.IsMatch(s);
        }

        public static double Scale(this double value, double scale, double begin, double end)
        {
            return (value / scale) * (end - begin) + begin;
        }

        public static Rect DrawText(DrawingContext dc, string s, double X, double Y, TextAlignment align, VerticalAlignment vAlign, double pixelsPerDip, Brush brush = null)
        {
            FormattedText ft = new FormattedText(s, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("Verdana"), 32, brush ?? Brushes.Black, pixelsPerDip);
            ft.TextAlignment = align;

            double vo = 0;
            double to = 0;
            if (vAlign == VerticalAlignment.Bottom) vo = ft.Height;
            else if (vAlign == VerticalAlignment.Center) vo = (ft.Height / 2);
            if (align == TextAlignment.Center) to = ft.Width / 2;
            else if (align == TextAlignment.Right) to = ft.Width;

            dc.DrawText(ft, new Point(X, Y - vo));
            return new Rect(X - to, Y - vo, ft.Width, ft.Height);
        }

        public static string Serialise(this object o)
        {
            if (o == null) return null;

            MemoryStream ms = new MemoryStream();
            BinaryFormatter f = new BinaryFormatter();
            f.Serialize(ms, o);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static object DeSerialise(this string s)
        {
            if (s == "") return null;

            MemoryStream ms = new MemoryStream(Convert.FromBase64String(s));
            BinaryFormatter f = new BinaryFormatter();
            return f.Deserialize(ms);
        }

        public static string XmlEncode(this string xml)
        {
            return HttpUtility.HtmlEncode(xml);
        }

        public static string XmlDecode(this string xml)
        {
            return HttpUtility.HtmlDecode(xml);
        }

        public static string AttributeValue(this XmlElement element, string attributeName)
        {
            var ve = element?.Attributes[attributeName];
            if (ve == null) return null;
            return ve?.Value;
        }

        public static int? AttributeIntValue(this XmlElement element, string attributeName)
        {
            int i;
            var ve = element?.Attributes[attributeName];
            if (ve != null && int.TryParse(ve.Value, out i)) return i;
            return null;
        }

        public static string ElementValue(this XElement element, string elementName)
        {
            var ve = element?.Element(elementName);
            return ve?.Value;
        }

        public static string ElementValue(this XElement element, string elementName, int index)
        {
            var ve = element?.Elements(elementName)?.ElementAtOrDefault(index);
            return ve?.Value;
        }

        public static int ElementIntValue(this XElement element, string elementName)
        {
            var ve = element?.Element(elementName);
            int i;
            return int.TryParse(ve?.Value, out i) ? i : 0;
        }

        public static bool ElementBoolValue(this XElement element, string elementName)
        {
            var ve = element?.Element(elementName);
            bool b;
            return bool.TryParse(ve?.Value, out b) ? b : false;
        }

        public static XmlNode CDataWrap(this string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("Message")).AppendChild(doc.CreateCDataSection(message));
            return doc.FirstChild;
        }

        public static string ToFriendlyString(this TimeSpan t)
        {
            string output = "";
            if (t.Days > 0) output += (output != "" ? ", " : "") + t.Days + " Day" + (t.Days > 1 ? "s" : "");
            if (t.Hours > 0) output += (output != "" ? ", " : "") + t.Hours + " Hour" + (t.Hours > 1 ? "s" : "");
            if (t.Minutes > 0) output += (output != "" ? ", " : "") + t.Minutes + " Minute" + (t.Minutes > 1 ? "s" : "");
            if (t.Seconds > 0) output += (output != "" ? ", " : "") + t.Seconds + " Second" + (t.Seconds > 1 ? "s" : "");
            if (t.Milliseconds > 0) output += (output != "" ? ", " : "") + t.Milliseconds + " Millisecond" + (t.Milliseconds > 1 ? "s" : "");
            return output;
        }

        public static bool In<t>(this t o, params t[] n)
        {
            return n.Contains(o);
        }

        public static bool NotIn<t>(this t o, params t[] n)
        {
            return !o.In(n);
        }
    }
}
