using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AssetBuilder.Classes
{
    public class ABEnvironment
    {
        public string Host {
            get
            {
                var split = Url.Split('/');
                return split.Length >= 3 ? split[2] : null;
            }
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public bool SSLAlso { get; set; }
        public bool Encrypted { get; set; }
        public string GetUrl(Dictionary<string, string> parameters)
        {
            var url = parameters.Aggregate(Url, (current, parameter) => current.Replace("{" + parameter.Key + "}", Encrypted ? HttpUtility.UrlEncode(parameter.Value.Encrypt()) : parameter.Value));
            url = Regex.Replace(url, "(?:[\\[])([^]]*)(?:[\\]])", m => Convert.ToBase64String(Encoding.UTF8.GetBytes(m.Groups[1].Value)));
            return url;
        }
        public ABEnvironment(string name, string type, string url, string ssl, string encrypted)
        {
            Name = name;
            Type = type;
            Url = url;
            bool b;
            if (bool.TryParse(ssl, out b)) SSLAlso = b;
            if (bool.TryParse(encrypted, out b)) Encrypted = b;
        }
    }
}
