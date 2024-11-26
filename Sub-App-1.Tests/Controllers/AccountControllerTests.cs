using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using Sub_App_1.Controllers;
using Sub_App_1.Models;
using Xunit;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


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
    _userManagerMock = new Mock<UserManager<IdentityUser>>(
        new Mock<IUserStore<IdentityUser>>().Object,
        new Mock<IOptions<IdentityOptions>>().Object,
        new Mock<IPasswordHasher<IdentityUser>>().Object,
        new IUserValidator<IdentityUser>[0],
        new IPasswordValidator<IdentityUser>[0],
        new Mock<ILookupNormalizer>().Object,
        new Mock<IdentityErrorDescriber>().Object,
        new Mock<IServiceProvider>().Object,
        new Mock<ILogger<UserManager<IdentityUser>>>().Object
    );

    _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
        _userManagerMock.Object,
        new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
        new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
        new Mock<IOptions<IdentityOptions>>().Object,
        new Mock<ILogger<SignInManager<IdentityUser>>>().Object,
        new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
        new Mock<IUserConfirmation<IdentityUser>>().Object
    );

    _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
        new Mock<IRoleStore<IdentityRole>>().Object,
        new IRoleValidator<IdentityRole>[0],
        new Mock<ILookupNormalizer>().Object,
        new Mock<IdentityErrorDescriber>().Object,
        new Mock<ILogger<RoleManager<IdentityRole>>>().Object
    );

    // Initialize AccountController with the mocked objects
    _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object);
        }

        // Update each test case to use _userManagerMock, _signInManagerMock, and _roleManagerMock directly.
        [Fact]
        public async Task Login_WithValidCredentialsAndRoles_RedirectsToProducts()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var user = new IdentityUser { UserName = username };
            var roles = new List<string> { UserRoles.RegularUser };

            _signInManagerMock
                .Setup(signIn => signIn.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userManagerMock
                .Setup(userManager => userManager.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(userManager => userManager.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Productsindex", result!.ActionName);
            Assert.Equal("Products", result.ControllerName);
        }

        [Fact]
        public async Task Login_WithValidCredentialsAndNoRoles_RedirectsToHome()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var user = new IdentityUser { UserName = username };
            var roles = new List<string>();

            _signInManagerMock
                .Setup(signIn => signIn.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userManagerMock
                .Setup(userManager => userManager.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(userManager => userManager.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }

        // Similarly, update all other test cases to directly use _userManagerMock, _signInManagerMock, and _roleManagerMock
        // Also, ensure each test accurately mocks the needed methods on these mocks directly instead of _userRepositoryMock.

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsErrorInView()
        {
            // Arrange
            var username = "testuser";
            var password = "InvalidPassword123";

            _signInManagerMock
                .Setup(signIn => signIn.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(IdentitySignInResult.Failed);

            // Act
            var result = await _controller.Login(username, password) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid username or password.", result.ViewData["Error"]);
            Assert.Equal("Index", result.ViewName);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsErrorInView()
        {
            // Arrange
            var username = "nonExistentUser";
            var password = "ValidPassword123";

            _signInManagerMock
                .Setup(signIn => signIn.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userManagerMock
                .Setup(userManager => userManager.FindByNameAsync(username))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var actionResult = await _controller.Login(username, password);
            var result = actionResult as ViewResult;

            // Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(result);
            Assert.Equal("Invalid username or password.", result!.ViewData["Error"]);
            Assert.Equal("Index", result.ViewName);
        }

        [Fact]
        public async Task Login_WithUnexpectedError_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";

            _signInManagerMock
                .Setup(signIn => signIn.PasswordSignInAsync(username, password, false, false))
                .Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Login(username, password));
        }
    }
}
