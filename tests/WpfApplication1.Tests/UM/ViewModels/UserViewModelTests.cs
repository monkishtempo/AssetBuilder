using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Asset_Builder.Tests.Helpers;
using AssetBuilder.UM.Models;
using AssetBuilder.UM.ViewModels;
using Xunit;

namespace Asset_Builder.Tests.UM.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class UserViewModelTests
    {
        private UserViewModel _model;

        private readonly AssetUser _user = new AssetUser
        {
            Company = new Company{ Id = 1, Name = "HH", Sprojoid = "1234-45-56789" },
            Password = "5ecr3tP455w0rd",
            Roles = new List<UserRole>
            {
                new UserRole{ Assigned = true, Name = "God", Id = 1, sprojoid = "H34v3n"},
                new UserRole{ Assigned = false, Name = "Devil", Id = 666, sprojoid = "H311"}
            },
            Sprojoid = "9999-222-222-7654",
            Status = "Active",
            UserName = "BOBOB",
            UserId = 1
        };

        [Fact]
        public void UserViewModel_PropertyValues()
        {
            _model = new UserViewModel(_user);

            Assert.Equal(_user.UserId, _model.UserId);
            Assert.Equal(_user.UserName, _model.UserName);
            Assert.Equal(_user.Sprojoid, _model.Sprojoid);
            Assert.Equal(_user.Company.Id, _model.Company.Id);
            Assert.Equal(_user.Company.Name, _model.Company.Name);
            Assert.Equal(_user.Company.Sprojoid, _model.Company.Sprojoid);
        }

        [Fact]
        public void UserViewModel_PropertyChangedEvents()
        {
            _model = new UserViewModel(_user);
            var tester = new NotifyPropertyChangedHelper(_model);

            _model.UserId = 99;
            _model.Sprojoid = "99";
            _model.UserName = "99";
            _model.Company = null;

            Assert.Equal(4, tester.Changes.Count);
            tester.AssertChange(0, "UserId");
            tester.AssertChange(1, "Sprojoid");
            tester.AssertChange(2, "UserName");
            tester.AssertChange(3, "Company");
        }
    }
}