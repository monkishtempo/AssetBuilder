using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.Services;
using AssetBuilder.UM.ViewModels;
using Moq;
using Xunit;

namespace Asset_Builder.Tests.UM.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class UserProfileViewModelTests
    {
        private readonly Mock<IUserManagerRepository> _repositoryMock = new Mock<IUserManagerRepository>();

        private UserProfileViewModel _model;

        private readonly UserInfoSessionLoginHistory _loginHistory = new UserInfoSessionLoginHistory
        {
            Current = new DateTime(2022, 01, 21, 11, 55, 30),
            Former = new DateTime(2022, 01, 20, 10, 45, 30),
            Previous = new DateTime(2022, 01, 19, 09, 35, 30),
            DomainLoginAccount = "HH/Me"
        };

        private readonly UserRoleAssignment _assignedRoles = new UserRoleAssignment
        {
            AlgoAdmin = true, AlgoBuilders = true, AlgoReviewers = false, AlgoTranslator = false, Comments = false
        };

        private readonly List<UserRoleAssignmentHistory> _personalAuditHistory = new List<UserRoleAssignmentHistory>
        {
            new UserRoleAssignmentHistory {Add = "Add", ModifiedBy = "ModifiedBy", Remove = "Remove", Role = "Role"}
        };

        private readonly List<UserAudit> _userAudits = new List<UserAudit>
        {
            new UserAudit {Date = new DateTime(2020, 01, 22, 07, 22, 59)}
        };

        private readonly List<UserAudit> _roleHistory = new List<UserAudit>
        {
            new UserAudit {Date = new DateTime(2019, 01, 22, 07, 22, 59)}
        };

        [Fact]
        public void OnLoad_UserDetailsAreRetrieved()
        {
            _repositoryMock.Setup(x => x.GetLoginHistory()).Returns(_loginHistory);
            _repositoryMock.Setup(x => x.GetRoleAssignment()).Returns(_assignedRoles);
            _repositoryMock.Setup(x => x.GetRoleAssignmentHistory()).Returns(_personalAuditHistory);
            _repositoryMock.Setup(x => x.GetAuditHistory()).Returns(_userAudits);
            _repositoryMock.Setup(x => x.GetPersonalAuditHistory()).Returns(_roleHistory);
            _model = new UserProfileViewModel(_repositoryMock.Object);

            Assert.Equal(_loginHistory.DomainLoginAccount, _model.AccountName);
            Assert.Equal(_loginHistory.Current, _model.CurrentLogin);
            Assert.Equal(_loginHistory.Previous, _model.PreviousLogin);
            Assert.Equal(_loginHistory.Former, _model.FormerLogin);
            Assert.Equal(_assignedRoles, _model.AssignedRoles);

            Assert.Collection(_model.RoleAssignmentHistory, x => Assert.Equal(x, _personalAuditHistory[0]));
            Assert.Collection(_model.RoleAuditHistory, x => Assert.Equal(x, _userAudits[0]));
            Assert.Collection(_model.RolePersonalAuditHistory, x => Assert.Equal(x, _roleHistory[0]));
        }
    }
}