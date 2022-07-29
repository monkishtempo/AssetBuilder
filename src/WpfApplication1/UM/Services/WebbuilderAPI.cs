using System;
using System.Collections.Generic;
using AssetBuilder.Properties;
using AssetBuilder.UM.Data;

namespace AssetBuilder.UM.Services
{
    public class WebbuilderAPI
    {
        private string webbuilderDomain = Settings.Default.WebService;

        private static UserSecurity _userSecurity;

        public WebbuilderAPI(UserSecurity userSecurity)
        {
            _userSecurity = userSecurity;
        }

        public T CallWebbuilderAPI<T>(ApiMethods methodName, List<InputParams> inputParams = null) where T: class
        {
            var then = DateTime.Now;
            var globalparams = "?sessionid=" + _userSecurity.SessionId + "&abname=" + _userSecurity.AssetBuilderUserName + "&windname=" + _userSecurity.WindowsUserName;
            var methodParams = "";

            if(inputParams != null)
            {
                foreach (var input in inputParams)
                {
                    methodParams += "&"+input.Name+"="+input.Value;
                }
            }

            var source = new Uri(new Uri(webbuilderDomain), $"UM/" + methodName + globalparams + methodParams).AbsoluteUri;
            var jnode = source.GetContent<T>();

            if(typeof(T).FullName == "AssetBuilder.JNode") DataAccess.AddLastCommand(source, jnode as JNode, DateTime.Now - then);
            return jnode;
        }
    }
}