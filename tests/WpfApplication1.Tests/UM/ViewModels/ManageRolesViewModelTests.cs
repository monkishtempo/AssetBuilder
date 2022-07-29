using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using AssetBuilder.Services;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.Services;
using AssetBuilder.UM.ViewModels;
using Moq;
using Xunit;

namespace Asset_Builder.Tests.UM.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class ManageRolesViewModelTests
    {
        private readonly Mock<IUserManagerRepository> _repositoryMock = new Mock<IUserManagerRepository>();

        private readonly Mock<IDialogService> _dialogMock = new Mock<IDialogService>();

        private ManageRolesViewModel _model;

        private readonly List<Company> _companies = new List<Company>
        {
            new Company {Id = 1, Name = "HealthHero", Sprojoid = "123456-12-123-123-1234"},
            new Company {Id = 2, Name = "Babylon", Sprojoid = "987654-98-987-987-9876"}
        };

        private readonly List<AssetUser> _users = new List<AssetUser>
        {
            new AssetUser {UserName = "Bob", Sprojoid = "1234", Roles = new List<UserRole>{new UserRole{Name = "Admin", Assigned = true}}}
        };

        [Fact]
        public void MissingRepository_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageRolesViewModel(null, _dialogMock.Object));
        }

        [Fact]
        public void MissingDialogService_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageRolesViewModel(_repositoryMock.Object, null));
        }

        [Fact]
        public void CompanyList_ReturnsDefaultWhenRepositoryRetrievesNothing()
        {
            const string expected = "No companies found.";
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            var actual = _model.Companies;

            Assert.Single(actual);
            Assert.Equal(actual[0].Name, expected);
            _repositoryMock.Verify(x => x.GetCompanies(), Times.Once);
        }

        [Fact]
        public void CompanyList_DoesNotContainDefaultWhenRepositoryReturnsValues()
        {
            var notExpected = new Company { Name = "No companies found." };
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            var actual = _model.Companies;

            Assert.NotNull(actual);
            Assert.DoesNotContain(notExpected, actual);
        }

        [Fact]
        public void WhenUserSelected_RoleTitleShowsTheirName()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            var expected = _model.EnvironmentUsers[0].UserName;

            var actual = _model.AssignedRoleTitle;

            Assert.Contains(expected, actual);
        }

        [Fact]
        public void WhenUserSelected_PasswordTitleShowsTheirName()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            var expected = _model.EnvironmentUsers[0].UserName;

            var actual = _model.ResetPasswordTitle;

            Assert.Contains(expected, actual);
        }

        [Fact]
        public void OnRefresh_WithCompanyAndUserSelected_CompanyListIsReloadedAndUserReselected()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.Refresh();

            // 2 calls for each - 1 on initial load, 1 on refresh
            _repositoryMock.Verify(x => x.GetCompanies(), Times.Exactly(2));
            _repositoryMock.Verify(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>()), Times.Exactly(2));
            Assert.Equal(_model.Companies[1], _model.SelectedCompany);
            Assert.True(_model.IsCompanySelected);
            Assert.Equal(_model.EnvironmentUsers[0], _model.SelectedUser);
            Assert.True(_model.IsUserSelected);
            Assert.Equal(_users[0].Roles, _model.UsersRoles);
        }

        [Fact]
        public void OnRefreshUsers_WithNoSelectedCompany_DoesNothing()
        {
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.IsCompanySelected = false;
            _model.RefreshUsers();

            _repositoryMock.Verify(x => x.GetCompanies(), Times.Never);
            _repositoryMock.Verify(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>()), Times.Never);
        }

        [Fact]
        public void OnRefreshUsers_WithCompanyAndUserSelected_CompanyListIsReloadedAndUserReselected()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.RefreshUsers();

            // 2 calls for Company Users - 1 on initial load, 1 on refresh
            _repositoryMock.Verify(x => x.GetCompanies(), Times.Once);
            _repositoryMock.Verify(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>()), Times.Exactly(2));
            Assert.Equal(_model.Companies[1], _model.SelectedCompany);
            Assert.True(_model.IsCompanySelected);
            Assert.Equal(_model.EnvironmentUsers[0], _model.SelectedUser);
            Assert.True(_model.IsUserSelected);
            Assert.Equal(_users[0].Roles, _model.UsersRoles);
        }

        [Fact]
        public void OnSave_WithCompanyAndUserSelected_RepositoryIsInvokedForEachRole()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _repositoryMock.Setup(x => x.UpdateRolesForUser(_users[0], It.IsAny<List<UserRole>>())).Returns(true);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.UpdateCommand.Execute(null);

            Assert.Equal(1, _model.EnvironmentUsers[0].ActiveRoleCount);
            _repositoryMock.Verify(x => x.UpdateRolesForUser(_users[0], It.IsAny<List<UserRole>>()), Times.Once);
        }

        [Fact]
        public void OnSaveSuccess_SuccessDialogIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _repositoryMock.Setup(x => x.UpdateRolesForUser(_users[0], It.IsAny<List<UserRole>>())).Returns(true);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.UpdateCommand.Execute(null);

            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update success", MessageBoxButton.OK, MessageBoxImage.Information), Times.Once);
        }

        [Fact]
        public void OnSaveFail_FailureDialogIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _repositoryMock.Setup(x => x.UpdateRolesForUser(_users[0], It.IsAny<List<UserRole>>())).Returns(false);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.UpdateCommand.Execute(null);

            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update failed", MessageBoxButton.OK, MessageBoxImage.Error), Times.Once);
        }

        [Fact]
        public void OnPasswordReset_ConfirmationPromptIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.ResetPasswordCommand.Execute(null);

            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Reset Password", MessageBoxButton.YesNo, MessageBoxImage.Question), Times.Once);
        }

        [Fact]
        public void OnPasswordResetSuccess_SuccessDialogIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _dialogMock.Setup(x => x.ShowMessageBox(It.IsAny<string>(), "Reset Password", MessageBoxButton.YesNo,
                MessageBoxImage.Question)).Returns(MessageBoxResult.Yes);
            _repositoryMock.Setup(x => x.PasswordReset(It.IsAny<string>(), It.IsAny<string>())).Returns((true, string.Empty));
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.ResetPasswordCommand.Execute(null);

            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update success", MessageBoxButton.OK, MessageBoxImage.Information), Times.Once);
        }

        [Fact]
        public void OnPasswordResetFailure_FailureDialogIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsersAndRoles(It.IsAny<Company>())).Returns(_users);
            _dialogMock.Setup(x => x.ShowMessageBox(It.IsAny<string>(), "Reset Password", MessageBoxButton.YesNo,
                MessageBoxImage.Question)).Returns(MessageBoxResult.Yes);
            _repositoryMock.Setup(x => x.PasswordReset(It.IsAny<string>(), It.IsAny<string>())).Returns((false, "ERROR"));
            _model = new ManageRolesViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.SelectedUser = _model.EnvironmentUsers[0];
            _model.IsCompanySelected = true;
            _model.IsUserSelected = true;
            _model.ResetPasswordCommand.Execute(null);

            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update failed", MessageBoxButton.OK, MessageBoxImage.Error), Times.Once);
        }
    }
}