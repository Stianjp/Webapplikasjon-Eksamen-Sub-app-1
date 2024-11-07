using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Sub_App_1.Controllers;
using Sub_App_1.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sub_App_1.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            // Opprett UserManager mock
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object, null, null, null, null, null, null, null, null
            );

            // Opprett SignInManager mock
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                null, null, null, null
            );

            // Opprett RoleManager mock
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null
            );

            // Initialiser AccountController med de mockede objektene
            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldRedirectToDashboard()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var user = new IdentityUser { UserName = username };

            _signInManagerMock
                .Setup(sm => sm.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock
                .Setup(um => um.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoles.RegularUser });

            // Act
            var result = await _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dashboard", result.ActionName);
            Assert.Equal("RegularUser", result.ControllerName);
        }
    }
}
