using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class NewUserViewModelTests
    {
        private readonly Mock<IUserManagerRepository> _repositoryMock = new Mock<IUserManagerRepository>();

        private readonly Mock<IDialogService> _dialogMock = new Mock<IDialogService>();

        private NewUserViewModel _model;

        private readonly List<Company> _companies = new List<Company>
        {
            new Company {Id = 1, Name = "HealthHero", Sprojoid = "123456-12-123-123-1234"},
            new Company {Id = 2, Name = "Babylon", Sprojoid = "987654-98-987-987-9876"}
        };

        private readonly AssetUser _validUser = new AssetUser {UserName = "Bob", Password = "5hh53cr3tP455w0rd", Company = new Company { Name = "Meds'R'Us" }};

        [Fact]
        public void MissingUser_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new NewUserViewModel(null, _repositoryMock.Object, _dialogMock.Object));
        }

        [Fact]
        public void MissingRepository_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new NewUserViewModel(AssetUser.CreateNewUser(), null, _dialogMock.Object));
        }

        [Fact]
        public void MissingDialogService_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new NewUserViewModel(AssetUser.CreateNewUser(), _repositoryMock.Object, null));
        }

        [Fact]
        public void CompanyList_ReturnsDefaultWhenRepositoryRetrievesNothing()
        {
            const string expected = "No companies found.";
            _model = new NewUserViewModel(AssetUser.CreateNewUser(), _repositoryMock.Object, _dialogMock.Object);

            var actual = _model.CompanyOptions;

            Assert.Single(actual);
            Assert.Equal(actual[0].Name, expected);
            _repositoryMock.Verify(x => x.GetCompanies(), Times.Once);
        }

        [Fact]
        public void CompanyList_DoesNotContainDefaultWhenRepositoryReturnsValues()
        {
            var notExpected = new Company { Name = "No companies found." };
            _repositoryMock.Setup(x => x.GetCompanies()).Returns(_companies);
            _model = new NewUserViewModel(AssetUser.CreateNewUser(), _repositoryMock.Object, _dialogMock.Object);

            var actual = _model.CompanyOptions;

            Assert.NotNull(actual);
            Assert.DoesNotContain(notExpected, actual);
        }

        [Fact]
        public void OnSave_WithInvalidUser_ThrowsException()
        {
            _model = new NewUserViewModel(AssetUser.CreateNewUser(), _repositoryMock.Object, _dialogMock.Object);
            _model.UserName = string.Empty;
            _model.Password = string.Empty;

            Assert.Throws<InvalidOperationException>(() => _model.SaveCommand.Execute(null));
        }

        [Fact]
        public void OnSave_WithValidUser_RepositoryIsInvokedCurrentUserIsCleared()
        {
            _repositoryMock.Setup(x => x.AddUser(_validUser)).Returns((true, string.Empty));
            _validUser.Company = _companies[0];
            _model = new NewUserViewModel(_validUser, _repositoryMock.Object, _dialogMock.Object);

            _model.SaveCommand.Execute(null);

            _repositoryMock.Verify(x => x.AddUser(_validUser), Times.Once());
            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update success", MessageBoxButton.OK, MessageBoxImage.Information), Times.Once);
            Assert.Null(_model.UserName);
            Assert.Null(_model.Password);
            Assert.Null(_model.UserCompany);
        }

        [Fact]
        public void OnSave_WithValidUser_RepositoryFailsCurrentUserDetailsRemain()
        {
            _repositoryMock.Setup(x => x.AddUser(_validUser)).Returns((false, "ERROR"));
            _validUser.Company = _companies[1];
            _model = new NewUserViewModel(_validUser, _repositoryMock.Object, _dialogMock.Object);

            _model.SaveCommand.Execute(null);

            _repositoryMock.Verify(x => x.AddUser(_validUser), Times.Once);
            _dialogMock.Verify(x => x.ShowMessageBox(It.IsAny<string>(), "Update failed", MessageBoxButton.OK, MessageBoxImage.Error), Times.Once);
            Assert.Equal(_validUser.UserName, _model.UserName);
            Assert.Equal(_validUser.Password, _model.Password);
            Assert.Equal(_companies[1], _model.UserCompany);
        }

        [Fact]
        public void CanSaveWithDefaultCompany_IsFalse()
        {
            _repositoryMock.Setup(x => x.AddUser(_validUser)).Returns((false, "ERROR"));
            _model = new NewUserViewModel(_validUser, _repositoryMock.Object, _dialogMock.Object);

            _model.UserCompany = new Company { Name = "No companies found." };
            var actual = _model.SaveCommand.CanExecute(null);

            Assert.False(actual);
        }

        [Theory]
        [InlineData("No companies found.")]
        [InlineData("(Select a company)")]
        public void CompanyIsValidated_ErrorMessageReturnedWhenNoCompanySelected(string optionValue)
        {
            const string expected = "No company selected.";
            _model = new NewUserViewModel(_validUser, _repositoryMock.Object, _dialogMock.Object);
            _model.UserCompany = new Company {Name = optionValue};

            var actual = ((IDataErrorInfo)_model)["UserCompany"];

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("A Company")]
        [InlineData("Some other company")]
        public void CompanyIsValidated_NullReturnedWhenCompanySelected(string optionValue)
        {
            _model = new NewUserViewModel(_validUser, _repositoryMock.Object, _dialogMock.Object);
            _model.UserCompany = new Company { Name = optionValue };

            var actual = ((IDataErrorInfo)_model)["UserCompany"];

            Assert.Null(actual);
        }
    }
}