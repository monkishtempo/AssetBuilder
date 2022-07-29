using System;
using System.Collections.Generic;
using System.Linq;
using AssetBuilder.Extensions;
using AssetBuilder.UM.Data;
using AssetBuilder.UM.Models;

namespace AssetBuilder.UM.Services
{
    public class UserManagerRepository : IUserManagerRepository
    {
        private readonly WebbuilderAPI _webBuilderApi;

        private readonly UserSecurity _currentUser;

        #region Database Initialise/Sync

        public bool SetupDatabase()
        {
            if (_currentUser.SchemaInitialised)
            {
                return true;
            }

            var inputParams = new List<InputParams>();
            var result = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.dbsetup, inputParams);

            return result.Success;
        }

        public (bool, bool) SynchroniseUsers()
        {
            if (!_currentUser.SchemaInitialised)
            {
                return (false, false);
            }

            var inputParams = new List<InputParams>();
            var result = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.usrsync, inputParams);

            return (result.Success, true);
        }
        #endregion

        #region User Profile

        public UserManagerRepository(UserSecurity currentUser)
        {
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));

            _currentUser = currentUser;
            _webBuilderApi = new WebbuilderAPI(currentUser);
        }

        public UserInfoSessionLoginHistory GetLoginHistory()
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userID", _currentUser.UserSprojoid)
            };

            var userInfoSessionLoginHistories = _webBuilderApi.CallWebbuilderAPI<List<UserInfoSessionLoginHistory>>(ApiMethods.userSessionLoginHistory, inputParams);

            return userInfoSessionLoginHistories.Any() ? userInfoSessionLoginHistories[0] : null;
        }

        public UserRoleAssignment GetRoleAssignment()
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userID", _currentUser.UserSprojoid)
            };

            var userRoleAssignment = _webBuilderApi.CallWebbuilderAPI<UserRoleAssignment>(ApiMethods.userOverviewRoleAssignment, inputParams);

            return userRoleAssignment;
        }

        public List<UserRoleAssignmentHistory> GetRoleAssignmentHistory()
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userID", _currentUser.UserSprojoid)
            };

            var userRoleAssignmentHistory = _webBuilderApi.CallWebbuilderAPI<List<UserRoleAssignmentHistory>>(ApiMethods.userOverviewRoleAssignmentHistory, inputParams);

            return userRoleAssignmentHistory;
        }

        public List<UserAudit> GetAuditHistory()
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userID", _currentUser.UserSprojoid)
            };

            var userAuditHistory = _webBuilderApi.CallWebbuilderAPI<List<UserAudit>>(ApiMethods.userOverviewAudit, inputParams);

            return userAuditHistory;
        }

        public List<UserAudit> GetPersonalAuditHistory()
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userID", _currentUser.UserSprojoid)
            };

            var userAuditHistory = _webBuilderApi.CallWebbuilderAPI<List<UserAudit>>(ApiMethods.myauditOverviewAudit, inputParams);

            return userAuditHistory;
        }

        #endregion User Profile

        #region New User

        public event EventHandler<UserAddedEventArgs> UserAdded;

        public List<Company> GetCompanies()
        {
            var companies = _webBuilderApi.CallWebbuilderAPI<List<Company>>(ApiMethods.getcompany);

            return companies;
        }

        public (bool, string) AddUser(AssetUser user)
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("companyId", user.Company.Sprojoid),
                new InputParams("userName", user.UserName),
                new InputParams("pwd", user.Password)
            };

            var result = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.processor_CreateNewUser, inputParams);
            if (!result.Success) return (false, result.Message.RemoveLineEndings());

            if (UserAdded != null) UserAdded(this, new UserAddedEventArgs(user));
            return (true, string.Empty);
        }

        #endregion New User

        #region Manage Roles

        public List<AssetUser> GetCompanyUsersAndRoles(Company company)
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("companyid", company.Sprojoid)
            };

            var companyUsers = _webBuilderApi.CallWebbuilderAPI<JNode>(ApiMethods.getusers, inputParams);
            var result = new List<AssetUser>();

            foreach (var user in companyUsers["Users"])
            {
                var employee = new AssetUser
                {
                    UserId = user["PK_User_ID"],
                    UserName = user["UName"],
                    Sprojoid = user["SPROJOID"],
                    Status = GetStatus(user["Orphaned"], user["Active"], user["Roles"].Count),
                    Company = company,
                    Roles = companyUsers["DBRoles"]
                        .Select(f => new UserRole
                        {
                            Id = f["PK_Role_ID"],
                            Name = f["Role_Name"],
                            Assigned = user["Roles"].Contains(f["PK_Role_ID"]),
                            sprojoid = f["SPROJOID"]
                        }).ToList()
                };

                result.Add(employee);
            }

            return result;
        }

        public bool UpdateRolesForUser(AssetUser user, List<UserRole> changedRoles)
        {
            if (user == null) return true;

            var overallResult = true;
            foreach (var role in changedRoles)
            {
                if (role.Assigned)
                {
                    // Enable role
                    var inputParams = new List<InputParams>
                    {
                        new InputParams("userId", user.Sprojoid),
                        new InputParams("roleId", role.sprojoid),
                        new InputParams("assigned", "true")
                    };
                    var enableResult = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.processor_UpdateUserRoles, inputParams);
                    overallResult &= enableResult.Success;
                    continue;
                }

                // Disable role
                var sqlParams = new List<InputParams>
                {
                    new InputParams("userId", user.Sprojoid),
                    new InputParams("roleId", role.sprojoid)
                };
                var disableResult = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.processor_UpdateUserRole_Remove, sqlParams);
                overallResult &= disableResult.Success;
            }

            return overallResult;
        }

        #endregion Manage Roles

        #region Manage User Company

        public List<AssetUser> GetCompanyUsers(Company company)
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("companyid", company.Sprojoid)
            };

            var companyUsers = _webBuilderApi.CallWebbuilderAPI<JNode>(ApiMethods.getusers, inputParams);
            var result = new List<AssetUser>();

            foreach (var user in companyUsers["Users"])
            {
                var employee = new AssetUser
                {
                    UserId = user["PK_User_ID"],
                    Sprojoid = user["SPROJOID"],
                    Company = company,
                    UserName = user["UName"],
                    Password = "DummyP455w0rd!F0r15V4lid"
                };

                result.Add(employee);
            }
            
            return result;
        }

        public bool MoveUserToCompany(string userId, string companyId)
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userID", userId),
                new InputParams("companyID", companyId)
            };
            var result = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.processor_MoveUserToCompany, inputParams);

            return result.Success;
        }

        #endregion Manage User Company

        #region Password Reset (Admin can reset other users passwords)

        public (bool, string) PasswordReset(string userName, string newPassword)
        {
            var inputParams = new List<InputParams>
            {
                new InputParams("userName", userName),
                new InputParams("pwd", newPassword)
            };
            var result = _webBuilderApi.CallWebbuilderAPI<ProcessStatus>(ApiMethods.processor_PasswordReset, inputParams);
            if (!result.Success) return (false, result.Message.RemoveLineEndings());

            return (true, string.Empty);
        }
        #endregion Password Reset

        private static string GetStatus(bool orphaned, bool active, int roleCount)
        {
            var userStatus = "Active";

            if (roleCount == 0)
            {
                userStatus += " (Sync Required)";
            }

            if (orphaned)
            {
                userStatus = "Orphaned";
                if (roleCount == 0)
                {
                    userStatus += " (Sync Required)";
                }
            }
            else if (!active)
            {
                userStatus = "Inactive";
            }

            return userStatus;
        }
    }
}