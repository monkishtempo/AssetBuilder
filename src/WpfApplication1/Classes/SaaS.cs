using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public sealed class SaaS
    {
        private static readonly SaaS instance = new SaaS();
        static SaaS() { }
        private SaaS() { }
        public static SaaS Instance { get { return instance; } }
        private JNode Token { get; set; }
        private DateTime Expiry { get; set; }
        public string token
        {
            get
            {
                if (Token == null || Expiry < DateTime.Now.AddMinutes(5))
                {
                    var c = Properties.Settings.Default.ClientID;
                    var s = Properties.Settings.Default.Secret;
                    var identity = new Uri(Properties.Settings.Default.SaaSIdentity);
                    var getToken = $"grant_type=client_credentials&client_id={c}&client_secret={s}";
                    var then = DateTime.Now;
                    Token = getToken.PostObject<JNode>(new Uri(identity, "connect/token").AbsoluteUri, new[] { ("Content-Type", "application/x-www-form-urlencoded") });
                    Expiry = DateTime.Now.AddSeconds(Token["expires_in"]);
                    DataAccess.AddLastCommand(getToken, Token, DateTime.Now - then);
                }
                return Token["access_token"];
            }
        }

        public async Task SavePropertyToSaas(string type, string id, string name, string value)
        {
            var split = type.Split(':');
            var algoNode = id.Split(':');
            var stype = split[0];
            var sid = split.Length > 1 ? split[1] : id;
            await SaveToSaas(
                $"TraversalService/TableOutput/Asset_Property/json/object/{type}/{id}/{name}?TextAsset=Bloat",
                $"/{Properties.Settings.Default.ClientID}/Properties/{stype}/{sid}/{name}",
                "value", value == "null" ? "DELETE" : "PUT", algoNode.Length > 1 ? $"?algoId={algoNode[0]}&nodeId={algoNode[1]}" : "");
        }

        public async Task SaveAssetToSaas(string type, int? AssetID)
        {
            await SaveToSaas(
                $"TraversalService/TableOutput/Asset_{type}/json/object/{AssetID}?TextAsset=Bloat",
                $"/{Properties.Settings.Default.ClientID}/{type}s/{AssetID}",
                type.ToLower());
        }

        private async Task SaveToSaas(string get, string put, string key, string method = "PUT", string query = "")
        {
            await Task.CompletedTask;
            var content = new Uri(Properties.Settings.Default.SaaSEndpoint);
            var endpoint = get;
            var url = new Uri(new Uri(Properties.Settings.Default.WebService), endpoint).AbsoluteUri;
            var p = content.AbsoluteUri + put;
            var headers = new[] { ("Content-Type", "application/json"), ("Authorization", $"Bearer {token}"), };
            var then = DateTime.Now;
            var audit = new { user = Environment.UserName, reason = Environment.MachineName + " - Asset Builder " + Window1.windowTitle };
            JNode data = null;
            if (method == "DELETE")
            {
                data = audit.PostObject<JNode>(p + query, headers, method);
            }
            else
            {
                var a = url.GetContent<JNode>();
                DataAccess.AddLastCommand(url, a, then - DateTime.Now);
                then = DateTime.Now;
                var obj = DataAccess.JsonDeSerialize(a[key].ToJson());
                obj.Add("auditData", audit);
                data = obj.PostObject<JNode>(p + query, headers, method);
            }
            DataAccess.AddLastCommand(p, data, then - DateTime.Now);
        }
    }
}
