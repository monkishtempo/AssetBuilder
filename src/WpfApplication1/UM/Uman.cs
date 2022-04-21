using AssetBuilder.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.UM
{
    class WebbuilderAPI
    {
        public enum ApiMethods
        {
            dbsetup,
            usrsync,
            getcompany,
            getuser,
            getusers,
            userprofile,
            userSessionLoginHistory,
            userOverviewRoleAssignment,
            userOverviewRoleAssignmentHistory,
            userOverviewAudit,
            myauditOverviewAudit,
            processor_UpdateUserRoles,
            processor_UpdateUserRole_Remove,
            processor_MoveUserToCompany,
            processor_PasswordReset,
            processor_CreateNewUser
        }
        public class InputParams
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public InputParams(string name, string value)
            {
                Name = name;
                Value = value;
            }

        }

        //private string webbuilderDomain = Settings.Default.WebService;
        private static string webbuilderDomain = "http://localhost:52636/";
        private static UserSecurity UserSecurity;

        public WebbuilderAPI(UserSecurity userSecurity)
        {
            UserSecurity = userSecurity;
        }
        public T CallWebbuilderAPI<T>(ApiMethods methodName, List<InputParams> inputParams = null) where T: class
        {
            var globalparams = "?sessionid=" + UserSecurity.sessionID + "&abname=" + UserSecurity.abUserName + "&windname=" + UserSecurity.windowsUserName;
            var methodParams = "";

            if(inputParams != null)
            {
                foreach (InputParams input in inputParams)
                {
                    methodParams += "&"+input.Name+"="+input.Value;
                }
            }

            var source = new Uri(new Uri(webbuilderDomain), $"UM/" + methodName.ToString() + globalparams + methodParams).AbsoluteUri;
            var jnode = source.GetContent<T>();

            return jnode;
        }

    }

    class Uman
    {
        public UserSecurity userSecurity { get; set; }
        public class Company
        {
            public string sprojoid { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public bool webbuilderHostedByHH { get; set; }

            public Company()
            {

            }

            public List<Company> GetCompanies(UserSecurity userSecurity)
            {
                //List<Company> companies = new List<Company>();
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                var companies = webbuilderAPI.CallWebbuilderAPI<List<Company>>(WebbuilderAPI.ApiMethods.getcompany);

                //foreach (var j in jnode)
                //{
                //    Company company = new Company();
                //    company.sprojoid = j["sprojoid"]; 
                //    company.id = (Int32)j["id"];
                //    company.name = j["name"];
                //    company.webbuilderHostedByHH = j["webbuilderHostedByHH"];

                //    companies.Add(company);
                //}

                return companies;
            }
        }
        public class UserInfo
        {
            public string sprojoid { get; set; }
            public int companyID { get; set; }
            public int userID { get; set; }
            public string userName { get; set; }
            public string userLoginType { get; set; }
            public int databaseUserID { get; set; }
            public bool active { get; set; }
            public bool orphaned { get; set; }
            public bool serverPrinciple { get; set; }
            public bool databasePrinciple { get; set; }
            public byte[] sid { get; set; }
            public List<UserRole> userRoles { get; set; }
            public UserInfo()
            {

            }
            public List<UserInfo> GetUserInfos(UserSecurity userSecurity, string companySpoid)
            {
                //List<UserInfo> userInfos = new List<UserInfo>();
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("companyid", companySpoid));

                var userInfos = webbuilderAPI.CallWebbuilderAPI<List<UserInfo>>(WebbuilderAPI.ApiMethods.getusers, prms);

                return userInfos;
            }
            public UserInfo GetUserInfo(UserSecurity userSecurity, string userSprojoid)
            {
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("userID", userSprojoid));

                var userInfo = webbuilderAPI.CallWebbuilderAPI<UserInfo>(WebbuilderAPI.ApiMethods.getuser, prms);

                //UserRole userRole = new UserRole();
                //UserInfo userInfo = new UserInfo
                //{
                //    companyID = jnode["companyID"],
                //    userID = jnode["userID"],
                //    sprojoid = jnode["sprojoid"],
                //    active = jnode["active"],
                //    databasePrinciple = jnode["databasePrinciple"],
                //    userLoginType = jnode["userLoginType"],
                //    databaseUserID = jnode["databaseUserID"],
                //    userName = jnode["userName"],
                //    orphaned = jnode["orphaned"],
                //    serverPrinciple = jnode["serverPrinciple"],
                //    userRoles = userRole.GetUserRoles(jnode["userRoles"])
                //};

                return userInfo;
            }

        }
        public class UserRole
        {
            public string sprojoid { get; set; }
            public int roleID { get; set; }
            public string roleName { get; set; }
            public int databaseRoleID { get; set; }
            public bool assignedToRoleInSproj { get; set; }
            public bool active { get; set; }
            public bool currentDbRole { get; set; }

            public List<UserRole> GetUserRoles(JNode jNode)
            {
                List<UserRole> userRoles = new List<UserRole>();

                foreach (var j in jNode)
                {
                    UserRole userRole = new UserRole
                    {
                        sprojoid = j["sprojoid"],
                        roleID = j["roleID"],
                        roleName = j["roleName"],
                        databaseRoleID = j["databaseRoleID"],
                        currentDbRole = j["currentDbRole"],
                        assignedToRoleInSproj = j["assignedToRoleInSproj"],
                        active = j["active"]
                    };

                    userRoles.Add(userRole);
                }

                return userRoles;
            }
        }
        public class UserInfoSessionLoginHistory
        {
            public string DomainLoginAccount { get; set; }
            public DateTime Current { get; set; }
            public DateTime Previous { get; set; }
            public DateTime Former { get; set; }
        }
        public class UserRoleAssignment
        {
            public bool AlgoEditor { get; set; }
            public bool AlgoTranslator { get; set; }
            public bool Comments { get; set; }
            public bool AlgoAdmin { get; set; }
            public bool AlgoBuilders { get; set; }
            public bool AlgoReviewers { get; set; }
        }
        public class UserRoleAssignmentHistory
        {
            public string ModifiedBy { get; set; }
            public string Role { get; set; }
            public string Add { get; set; }
            public string Remove { get; set; }
        }
        public class UserAudit
        {
            public string ProcessName { get; set; }
            public DateTime Date { get; set; }
            public string Summary { get; set; }
            public string Detail { get; set; }
            public string LastModifiedBy { get; set; }
        }
        public Uman(string assetbuilderLoginName)
        {
            userSecurity = new UserSecurity(assetbuilderLoginName);
        }

        public class OverviewHistory
        {
            public List<UserInfoSessionLoginHistory> UserInfoSessionLoginHistories { get; set; }
            public List<UserRoleAssignment> UserRoleAssignment { get; set; }
            public List<UserRoleAssignmentHistory> UserRoleAssignmentHistories { get; set; }
            public List<UserAudit> UserAudit { get; set; }
            public List<UserAudit> UserMyAudit { get; set; }
            public OverviewHistory(UserSecurity userSecurity, string userSprojoid)
            {
                UserInfoSessionLoginHistories = GetUserInfoSessionLoginHistories(userSecurity, userSprojoid);
                UserRoleAssignment = GetUserRoleAssignment(userSecurity, userSprojoid);
                UserRoleAssignmentHistories = GetUserRoleAssignmentHistories(userSecurity, userSprojoid);
                UserAudit = GetUserAudit(userSecurity, userSprojoid);
                UserMyAudit = GetUserMyAudit(userSecurity);
            }

            private List<UserInfoSessionLoginHistory> GetUserInfoSessionLoginHistories(UserSecurity userSecurity, string userSprojoid)
            {
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("userID", userSprojoid));

                var userInfoSessionLoginHistories = webbuilderAPI.CallWebbuilderAPI<List<UserInfoSessionLoginHistory>>(WebbuilderAPI.ApiMethods.userSessionLoginHistory, prms);

                return userInfoSessionLoginHistories;
            }
            private List<UserRoleAssignment> GetUserRoleAssignment(UserSecurity userSecurity, string userSprojoid)
            {
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("userID", userSprojoid));

                var userRoleAssignment = webbuilderAPI.CallWebbuilderAPI<UserRoleAssignment>(WebbuilderAPI.ApiMethods.userOverviewRoleAssignment, prms);

                List<UserRoleAssignment> userRoleAssignments = new List<UserRoleAssignment>();
                userRoleAssignments.Add(userRoleAssignment);

                return userRoleAssignments;
            }
            private List<UserRoleAssignmentHistory> GetUserRoleAssignmentHistories(UserSecurity userSecurity, string userSprojoid)
            {
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("userID", userSprojoid));

                var userRoleAssignmentHistory = webbuilderAPI.CallWebbuilderAPI<List<UserRoleAssignmentHistory>>(WebbuilderAPI.ApiMethods.userOverviewRoleAssignmentHistory, prms);

                return userRoleAssignmentHistory;
            }
            private List<UserAudit> GetUserAudit(UserSecurity userSecurity, string userSprojoid)
            {
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("userID", userSprojoid));

                var userAuditHistory = webbuilderAPI.CallWebbuilderAPI<List<UserAudit>>(WebbuilderAPI.ApiMethods.userOverviewAudit, prms);

                return userAuditHistory;
            }
            private List<UserAudit> GetUserMyAudit(UserSecurity userSecurity)
            {
                WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
                List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
                prms.Add(new WebbuilderAPI.InputParams("userID", userSecurity.UserSprojoid));

                var userAuditHistory = webbuilderAPI.CallWebbuilderAPI<List<UserAudit>>(WebbuilderAPI.ApiMethods.myauditOverviewAudit, prms);

                return userAuditHistory;
            }
        }
    }

    class UserSecurity
    {
        public bool SchemaInitialised { get; set; }
        public string sessionID { get; set; }
        public int sessionPKID { get; set; }
        public string windowsUserName { get; set; }
        public string abUserName { get; set; }
        public bool isServerAdmin { get; set; }
        public bool isAssetBuilderAdmin { get; set; }
        public string UserSprojoid { get; set; }

        public UserSecurity(string assetbuilderLoginName)
        {
            sessionID = Guid.NewGuid().ToString();
            windowsUserName = WindowsUserName();
            abUserName = assetbuilderLoginName;
            isServerAdmin = false;
            isAssetBuilderAdmin = false;

            Session();
        }
        private string WindowsUserName()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
        }
        private void Session()
        {
            //create a new session
            WebbuilderAPI webbuilderAPI = new WebbuilderAPI(this);
            JNode jnode = webbuilderAPI.CallWebbuilderAPI<JNode>(WebbuilderAPI.ApiMethods.userprofile);

            sessionID = jnode["sessionID"];
            sessionPKID = jnode["sessionPKID"];
            isServerAdmin = jnode["isServerAdmin"];
            isAssetBuilderAdmin = jnode["isAssetBuilderAdmin"];
            UserSprojoid = jnode["UserSprojoid"];
            SchemaInitialised = jnode["SchemaInitialised"];
        }        
    }

    class Processing
    {
        UM.UserSecurity userSecurity;

        public class ProcessStatus
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public ProcessStatusValue Value { get; set; }
        }
        public class ProcessStatusValue
        {
            private int intValue;
            private string strValue;

            public int IntValue 
            {
                get { return intValue; }
                set
                {
                    try
                    {
                        intValue = Convert.ToInt32(value);
                    }
                    catch(Exception inte)
                    {
                        intValue = 0;
                    }
                }
            }
            public string StrValue 
            { 
                get; 
                set; 
            }
        }
        public Processing(UM.UserSecurity _userSecurity)
        {
            userSecurity = _userSecurity;
        }
        public ProcessStatus DBSetup()
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();

            if (!userSecurity.SchemaInitialised)
            {
                return WebbuilderProcessor(WebbuilderAPI.ApiMethods.dbsetup, parameterList: prms);
            }
            else
            {
                return new ProcessStatus
                {
                    Success = false,
                    Message = "Database has already been initialised"
                };
            }
        }
        public ProcessStatus UserSync()
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();

            if (userSecurity.SchemaInitialised)
            {
                return WebbuilderProcessor(WebbuilderAPI.ApiMethods.usrsync, parameterList: prms);
            }
            else
            {
                return new ProcessStatus
                {
                    Success = false,
                    Message = "Database requires initialisation before synchronisation can take place."
                };
            }
        }
        public ProcessStatus UpdateUserRoles(string userId, string roleId, bool assigned)
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
            prms.Add(new WebbuilderAPI.InputParams("userId", userId.ToString()));
            prms.Add(new WebbuilderAPI.InputParams("roleId", roleId.ToString()));
            prms.Add(new WebbuilderAPI.InputParams("assigned", assigned.ToString().ToLower()));

            return WebbuilderProcessor(WebbuilderAPI.ApiMethods.processor_UpdateUserRoles, parameterList: prms);
        }
        public ProcessStatus UpdateUserRole_Remove(string userId, UM.Uman.UserRole userRole)
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
            prms.Add(new WebbuilderAPI.InputParams("userId", userId.ToString()));
            prms.Add(new WebbuilderAPI.InputParams("roleId", userRole.sprojoid.ToString()));

            return WebbuilderProcessor(WebbuilderAPI.ApiMethods.processor_UpdateUserRole_Remove, prms);
        }
        public ProcessStatus MoveUserEntity(string userId, string toCompanyID)
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
            prms.Add(new WebbuilderAPI.InputParams("userID", userId.ToString()));
            prms.Add(new WebbuilderAPI.InputParams("companyID", toCompanyID));

            return WebbuilderProcessor(WebbuilderAPI.ApiMethods.processor_MoveUserToCompany, prms);

        }
        public ProcessStatus UserPasswordReset(string userName, string password)
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
            prms.Add(new WebbuilderAPI.InputParams("userName", userName.ToString()));
            prms.Add(new WebbuilderAPI.InputParams("pwd", password.ToString()));

            return WebbuilderProcessor(WebbuilderAPI.ApiMethods.processor_PasswordReset, parameterList: prms);
        }
        public ProcessStatus CreateNewSqlUser(string companyId, string userName, string password)
        {
            List<WebbuilderAPI.InputParams> prms = new List<WebbuilderAPI.InputParams>();
            prms.Add(new WebbuilderAPI.InputParams("companyId", companyId));
            prms.Add(new WebbuilderAPI.InputParams("userName", userName.ToString()));
            prms.Add(new WebbuilderAPI.InputParams("pwd", password.ToString()));

            return WebbuilderProcessor(WebbuilderAPI.ApiMethods.processor_CreateNewUser, parameterList: prms);
        }
        private ProcessStatus WebbuilderProcessor(WebbuilderAPI.ApiMethods method, List<WebbuilderAPI.InputParams> parameterList = null)
        {
            WebbuilderAPI webbuilderAPI = new WebbuilderAPI(userSecurity);
            List<WebbuilderAPI.InputParams> inputParams = new List<WebbuilderAPI.InputParams>();

            if (parameterList != null)
            {
                foreach (WebbuilderAPI.InputParams input in parameterList)
                {
                    inputParams.Add(input);
                }
            }

            var procStatus = webbuilderAPI.CallWebbuilderAPI<ProcessStatus>(method, inputParams);

            //ProcessStatus procStatus = new ProcessStatus
            //{
            //    Success = jnode["Success"],
            //    Message = jnode["Message"],
            //    Value = new ProcessStatusValue 
            //    {
            //        IntValue = jnode["ReturnKeyValue"],
            //        StrValue = jnode["ReturnKeyValue"]
            //    }
            //};

            return procStatus;
        }

    }
}
