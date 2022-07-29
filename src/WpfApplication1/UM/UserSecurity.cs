using System;
using AssetBuilder.UM.Data;
using AssetBuilder.UM.Services;

namespace AssetBuilder.UM
{
    public class UserSecurity
    {
        public bool SchemaInitialised { get; set; }
        public string SessionId { get; set; }
        public int SessionPkId { get; set; }
        public string WindowsUserName { get; set; }
        public string AssetBuilderUserName { get; set; }
        public bool IsServerAdmin { get; set; }
        public bool IsAssetBuilderAdmin { get; set; }
        public string UserSprojoid { get; set; }

        public UserSecurity(string assetBuilderLoginName)
        {
            SessionId = Guid.NewGuid().ToString();
            WindowsUserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            AssetBuilderUserName = assetBuilderLoginName;

            CreateSession();
        }

        private void CreateSession()
        {
            var webBuilderApi = new WebbuilderAPI(this);
            var jNode = webBuilderApi.CallWebbuilderAPI<JNode>(ApiMethods.userprofile);

            SessionId = jNode["sessionID"];
            SessionPkId = jNode["sessionPKID"];
            IsServerAdmin = jNode["isServerAdmin"];
            IsAssetBuilderAdmin = jNode["isAssetBuilderAdmin"];
            UserSprojoid = jNode["UserSprojoid"];
            SchemaInitialised = jNode["SchemaInitialised"];
        }        
    }
}