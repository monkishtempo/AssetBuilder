using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.UserManagement
{
    public class Userman
    {


        public Userman()
        {

        }

        public class Company
        {
            private string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Robin"].ConnectionString;
            dcp7708.mssql.foundation foundation;

            public int id { get; set; }
            public string name { get; set; }
            public bool webbuilderHostedByHH { get; set; }

            public Company()
            {
                foundation = new dcp7708.mssql.foundation();
            }
            public List<Company> GetCompanies()
            {
                List<Company> companies = new List<Company>();

                foundation.connectionString = ConnectionString;

                dcp7708.mssql.foundation.strSqlInput_Statement statement = new dcp7708.mssql.foundation.strSqlInput_Statement
                {
                    Statement = SqlGetCompanies()
                };
                dcp7708.mssql.foundation.strSqlInput sqlIN = new dcp7708.mssql.foundation.strSqlInput
                {
                    connectionString = foundation.connectionString,
                    optionalStatement = statement
                };

                try
                {
                    foundation.Initialize(foundation.connectionString);
                    System.Data.DataTable usr = foundation.DataTable(sqlIN).Table[0];

                    foreach (System.Data.DataRow row in usr.Rows)
                    {
                        Company company = new Company
                        {
                            id = Convert.ToInt32(row["PK_Company_ID"].ToString()),
                            name = row["Company_Name"].ToString()
                        };

                        companies.Add(company);
                    }
                }
                catch (Exception e)
                {

                }

                return companies;
            }

            private string SqlGetCompanies()
            {
                string sql = @"
SELECT * 
FROM sprojadm.COMPANY
ORDER BY Company_Name
";

                return sql;
            }
        }
        public class UserInfo
        {
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
        }
        public class UserRole
        {
            public int roleID { get; set; }
            public string roleName { get; set; }
            public int databaseRoleID { get; set; }
            public bool assignedToRoleInSproj { get; set; }
            public bool active { get; set; }
            public bool currentDbRole { get; set; }
        }

        public List<Company> GetCompanies(int excludeCompanyID=0)
        {

            List<Company> companies = new List<Company>();

            return companies;

        }
        public List<UserInfo> GetUserInfo(int companyID)
        {

            List<UserInfo> userInfos = new List<UserInfo>();

            var userID = 0;

            UserInfo userInfo = new UserInfo
            {
                userRoles = getUserRoles(userID)
            };

            userInfos.Add(userInfo);

            return userInfos;

        }

        private List<UserRole> getUserRoles(int UserId)
        {
            List<UserRole> userRoles = new List<UserRole>();

            return userRoles;
        }

    }
}
