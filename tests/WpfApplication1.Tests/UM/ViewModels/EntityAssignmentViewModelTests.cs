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
    public class EntityAssignmentViewModelTests
    {
        private readonly Mock<IUserManagerRepository> _repositoryMock = new Mock<IUserManagerRepository>();

        private readonly Mock<IDialogService> _dialogMock = new Mock<IDialogService>();

        private EntityAssignmentViewModel _model;

        private readonly List<Company> _companies = new List<Company>
        {
            new Company {Id = 1, Name = "HealthHero", Sprojoid = "123456-12-123-123-1234"},
            new Company {Id = 2, Name = "Babylon", Sprojoid = "987654-98-987-987-9876"}
        };

        private readonly List<AssetUser> _users = new List<AssetUser>
        {
            new AssetUser {UserName = "Bob", Sprojoid = "1234" }
        };

        [Fact]
        public void MissingRepository_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityAssignmentViewModel(null, _dialogMock.Object));
        }

        [Fact]
        public void MissingDialogService_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityAssignmentViewModel(_repositoryMock.Object, null));
        }

        [Fact]
        public void CompanyList_ReturnsDefaultWhenRepositoryRetrievesNothing()
        {
            const string expected = "No companies found.";
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            var actual = _model.Companies;

            Assert.Single(actual);
            Assert.Equal(actual[0].Name, expected);
            _repositoryMock.Verify(x => x.GetCompanies(), Times.Once);
        }

        [Fact]
        public void CompanyList_DoesNotContainDefaultWhenRepositoryReturnsValues()
        {
            var notExpected = new Company {Name = "No companies found."};
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            var actual = _model.Companies;

            Assert.NotNull(actual);
            Assert.DoesNotContain(notExpected, actual);
        }

        [Fact]
        public void OnRefresh_WithCompanySelected_CompanyListIsReloadedAndReselected()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[1];
            _model.IsCompanySelected = true;
            _model.Refresh();

            // 2 calls for each - 1 on initial load, 1 on refresh
            _repositoryMock.Verify(x => x.GetCompanies(), Times.Exactly(2));
            _repositoryMock.Verify(x => x.GetCompanyUsers(It.IsAny<Company>()), Times.Exactly(2));
            Assert.Equal(_model.Companies[1], _model.SelectedCompany);
            Assert.True(_model.IsCompanySelected);
        }

        [Fact]
        public void OnSave_WithCompanyAndUserSelected_RepositoryIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _repositoryMock.Setup(x => x.MoveUserToCompany(_users[0].Sprojoid, _companies[1].Sprojoid)).Returns(true);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[0];
            _model.DestinationCompany = _model.Companies[1];
            _model.EnvironmentUsers[0].IsUserSelected = true;
            _model.UpdateCommand.Execute(null);

            _repositoryMock.Verify(x => x.MoveUserToCompany(_users[0].Sprojoid, _companies[1].Sprojoid), Times.Once);
            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update Success", MessageBoxButton.OK, MessageBoxImage.Information), Times.Once);
            Assert.False(_model.EnvironmentUsers[0].IsUserSelected);
        }

        [Fact]
        public void OnSave_WithCompanyAndUserSelectedAndSaveFails_UserDialogIsInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _repositoryMock.Setup(x => x.MoveUserToCompany(_users[0].Sprojoid, _companies[1].Sprojoid)).Returns(false);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[0];
            _model.DestinationCompany = _model.Companies[1];
            _model.EnvironmentUsers[0].IsUserSelected = true;
            _model.UpdateCommand.Execute(null);

            _repositoryMock.Verify(x => x.MoveUserToCompany(_users[0].Sprojoid, _companies[1].Sprojoid), Times.Once);
            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update failed", MessageBoxButton.OK, MessageBoxImage.Error), Times.Once);
            Assert.True(_model.EnvironmentUsers[0].IsUserSelected);
        }

        [Fact]
        public void OnSave_WithNoSourceCompanySelected_RepositoryIsNotInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.DestinationCompany = _model.Companies[1];
            _model.UpdateCommand.Execute(null);

            Assert.NotNull(_model.DestinationCompany);
            _repositoryMock.Verify(x => x.MoveUserToCompany(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void OnSave_WithNoDestination_RepositoryIsNotInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[0];
            _model.EnvironmentUsers[0].IsUserSelected = true;
            _model.UpdateCommand.Execute(null);

            Assert.Null(_model.DestinationCompany);
            _repositoryMock.Verify(x => x.MoveUserToCompany(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void OnSave_WithCompanyButNoUserSelected_RepositoryIsNotInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[0];
            _model.DestinationCompany = _model.Companies[1];
            _model.UpdateCommand.Execute(null);

            Assert.NotNull(_model.SelectedCompany);
            Assert.NotNull(_model.DestinationCompany);
            _repositoryMock.Verify(x => x.MoveUserToCompany(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void OnSave_WithSourceCompanyEqualToDestinationCompany_RepositoryIsNotInvoked()
        {
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _repositoryMock.Setup(x => x.GetCompanyUsers(It.IsAny<Company>())).Returns(_users);
            _model = new EntityAssignmentViewModel(_repositoryMock.Object, _dialogMock.Object);

            _model.SelectedCompany = _model.Companies[0];
            _model.DestinationCompany = _model.Companies[0];
            _model.UpdateCommand.Execute(null);

            Assert.NotNull(_model.SelectedCompany);
            Assert.NotNull(_model.DestinationCompany);
            Assert.Equal(_model.SelectedCompany, _model.DestinationCompany);
            _repositoryMock.Verify(x => x.MoveUserToCompany(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}