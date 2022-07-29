using System;
using System.Collections.Generic;
using AssetBuilder.UM.Models;

namespace AssetBuilder.UM.Services
{
    public interface IUserManagerRepository
    {
        bool SetupDatabase();
        (bool, bool) SynchroniseUsers();
        UserInfoSessionLoginHistory GetLoginHistory();
        UserRoleAssignment GetRoleAssignment();
        List<UserRoleAssignmentHistory> GetRoleAssignmentHistory();
        List<UserAudit> GetAuditHistory();
        List<UserAudit> GetPersonalAuditHistory();
        event EventHandler<UserAddedEventArgs> UserAdded;
        List<Company> GetCompanies();
        (bool, string) AddUser(AssetUser user);
        List<AssetUser> GetCompanyUsersAndRoles(Company company);
        List<AssetUser> GetCompanyUsers(Company company);
        bool UpdateRolesForUser(AssetUser user, List<UserRole> changedRoles);
        bool MoveUserToCompany(string userId, string companyId);
        (bool, string) PasswordReset(string userName, string newPassword);
    }
}