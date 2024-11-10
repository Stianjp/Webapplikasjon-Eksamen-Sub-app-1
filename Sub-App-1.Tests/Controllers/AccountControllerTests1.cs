using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using Sub_App_1.Controllers;
using Sub_App_1.DAL.Interfaces;
using Sub_App_1.Models;
using Xunit;

// Alias for SignInResult from Identity
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Sub_App_1.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _controller = new AccountController(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentialsAndRoles_RedirectsToProducts()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var user = new IdentityUser { UserName = username };
            var roles = new List<string> { UserRoles.RegularUser };

            _userRepositoryMock
                .Setup(repo => repo.LoginAsync(username, password))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userRepositoryMock
                .Setup(repo => repo.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(repo => repo.GetRolesAsync(user))
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
            var roles = new List<string>(); // empty list, no roles

            _userRepositoryMock
                .Setup(repo => repo.LoginAsync(username, password))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userRepositoryMock
                .Setup(repo => repo.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(repo => repo.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsLoginViewWithError()
        {
            // Arrange
            var username = "testuser";
            var password = "InvalidPassword";
            _userRepositoryMock
                .Setup(repo => repo.LoginAsync(username, password))
                .ReturnsAsync(IdentitySignInResult.Failed);

            // Act
            var result = await _controller.Login(username, password) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ViewName);
            Assert.Equal("Invalid username or password.", result.ViewData["Error"]);
        }

        // Negative test: Register with missing username field
        [Fact]
        public async Task Register_WithMissingUsername_ReturnsModelError()
        {
            // Arrange
            var username = ""; // Missing username
            var password = "ValidPassword123";
            var confirmPassword = "ValidPassword123";
            var role = UserRoles.RegularUser;

            // Act
            var result = await _controller.Register(username, password, confirmPassword, role) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.ViewData.ModelState.ContainsKey(string.Empty));
            Assert.Contains("cannot be null or empty", result.ViewData.ModelState[string.Empty].Errors.First().ErrorMessage);
        }

        // Negative test: Register with unauthorized role
        [Fact]
        public async Task Register_WithUnauthorizedRole_ReturnsError()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var confirmPassword = "ValidPassword123";
            var unauthorizedRole = UserRoles.Administrator;

            _userRepositoryMock
                .Setup(repo => repo.RegisterAsync(username, password))
                .ReturnsAsync(IdentityResult.Success);

            _userRepositoryMock
                .Setup(repo => repo.FindByNameAsync(username))
                .ReturnsAsync(new IdentityUser { UserName = username });

            _userRepositoryMock
                .Setup(repo => repo.DeleteUserAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Success);  // Ensures DeleteUserAsync returns immediately

            // Act
            var result = await _controller.Register(username, password, confirmPassword, unauthorizedRole) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.ViewData.ModelState.ContainsKey(string.Empty));
            Assert.Equal("Error during registration.", result.ViewData["Error"]);
        }

        // Negative test: Register with mismatched passwords
        [Fact]
        public async Task Register_WithMismatchedPasswords_ReturnsModelError()
        {
            // Arrange
            var username = "testuser";
            var password = "Password123";
            var confirmPassword = "DifferentPassword123"; // Different passwords
            var role = UserRoles.RegularUser;

            // Act
            var result = await _controller.Register(username, password, confirmPassword, role) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.ViewData.ModelState.ContainsKey(string.Empty));
            Assert.Contains("Passwords do not match", result.ViewData.ModelState[string.Empty].Errors.First().ErrorMessage);
        }

        // Null reference test: Login with null user
        [Fact]
        public async Task Login_WithNullUser_ReturnsHomeIndex()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";

            _userRepositoryMock
                .Setup(repo => repo.LoginAsync(username, password))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userRepositoryMock
                .Setup(repo => repo.FindByNameAsync(username))
                .ReturnsAsync((IdentityUser)null); // Simulate null user

            // Act
            var result = await _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }

        // Null reference test: Login with null roles
        [Fact]
        public async Task Login_WithNullRoles_ReturnsHomeIndex()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var user = new IdentityUser { UserName = username };

            _userRepositoryMock
                .Setup(repo => repo.LoginAsync(username, password))
                .ReturnsAsync(IdentitySignInResult.Success);

            _userRepositoryMock
                .Setup(repo => repo.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(repo => repo.GetRolesAsync(user))
                .ReturnsAsync((IList<string>)null); // Simulate null roles

            // Act
            var result = await _controller.Login(username, password) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }

        // Null reference test: Register with null user
        [Fact]
        public async Task Register_WithNullUser_ReturnsError()
        {
            // Arrange
            var username = "testuser";
            var password = "ValidPassword123";
            var confirmPassword = "ValidPassword123";
            var role = UserRoles.RegularUser;

            _userRepositoryMock
                .Setup(repo => repo.RegisterAsync(username, password))
                .ReturnsAsync(IdentityResult.Success);

            _userRepositoryMock
                .Setup(repo => repo.FindByNameAsync(username))
                .ReturnsAsync((IdentityUser)null); // Simulate null user after registration

            // Act
            var result = await _controller.Register(username, password, confirmPassword, role) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.ViewData.ModelState.ContainsKey(string.Empty));
            Assert.Equal("Error during registration.", result.ViewData["Error"]);
        }
    }
}
