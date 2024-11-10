/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using Sub_App_1.Controllers;
using Sub_App_1.DAL.Interfaces;
using Sub_App_1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics;

// Alias to avoid conflicts with IdentityResult
using IdentityResultAlias = Microsoft.AspNetCore.Identity.IdentityResult;

namespace Sub_App_1.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            // Mock the IUserRepository dependency
            _userRepositoryMock = new Mock<IUserRepository>();

            // Instantiate the AdminController with the mock repository
            _controller = new AdminController(_userRepositoryMock.Object);
        
            // Set up TempData
            _controller.TempData = new Mock<ITempDataDictionary>().Object;
        }

        // Test: UserManager - Verify that the method returns the correct view with users and their roles
        [Fact]
        public async Task UserManager_ReturnsViewWithUsersAndRoles()
        {
            // Arrange: Create mock users and roles
            var users = new List<IdentityUser>
            {
                new IdentityUser { Id = "1", UserName = "user1" },
                new IdentityUser { Id = "2", UserName = "user2" }
            };

            var roles1 = new List<string> { "Role1" };
            var roles2 = new List<string> { "Role2" };

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync())
                .ReturnsAsync(users);

            _userRepositoryMock.Setup(repo => repo.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "1")))
                .ReturnsAsync(roles1);

            _userRepositoryMock.Setup(repo => repo.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "2")))
                .ReturnsAsync(roles2);

            // Act: Call the UserManager method
            var result = await _controller.UserManager() as ViewResult;

            // Assert: Verify the returned view and its model
            Assert.NotNull(result);
            var model = result.Model as List<UserWithRolesViewModel>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
            Assert.Equal("user1", model[0].Username);
            Assert.Equal("Role1", model[0].Roles[0]);
        }

        // Test: EditUser (GET) - Verify that a valid user ID returns the correct view with user roles
        [Fact]
        public async Task EditUser_ValidUserId_ReturnsViewWithUserRoles()
        {
            // Arrange: Mock user, roles, and all roles in the system
            var user = new IdentityUser { Id = "1", UserName = "testuser" };
            var roles = new List<string> { "Role1", "Role2" };
            var allRoles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Role1" },
                new IdentityRole { Name = "Role2" },
                new IdentityRole { Name = "Role3" }
            };

            _userRepositoryMock.Setup(repo => repo.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(repo => repo.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _userRepositoryMock.Setup(repo => repo.GetAllRolesAsync())
                .ReturnsAsync(allRoles);

            // Act: Call the EditUser method with a valid ID
            var result = await _controller.EditUser("1") as ViewResult;

            // Assert: Verify the view and its model
            Assert.NotNull(result);
            var model = result.Model as UserWithRolesViewModel;
            Assert.NotNull(model);
            Assert.Equal("testuser", model.Username);
            Assert.Equal(2, model.Roles.Count);
        }

        // Test: EditUser (GET) - Verify that an invalid user ID returns NotFound
        [Fact]
        public async Task EditUser_InvalidUserId_ReturnsNotFound()
        {
            // Arrange: Mock FindByIdAsync to return null
            _userRepositoryMock.Setup(repo => repo.FindByIdAsync("invalid-id"))
                .ReturnsAsync((IdentityUser)null);

            // Act: Call the EditUser method with an invalid ID
            var result = await _controller.EditUser("invalid-id");

            // Assert: Verify that NotFound is returned
            Assert.IsType<NotFoundResult>(result);
        }

        // Test: DeleteUserConfirmed - Verify that a valid user deletion redirects to UserManager
        [Fact]
        public async Task DeleteUserConfirmed_ValidUser_RedirectsToUserManager()
        {
            // Arrange: Mock a valid user and successful deletion
            var user = new IdentityUser { Id = "1", UserName = "testuser" };

            _userRepositoryMock.Setup(repo => repo.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(user))
                .ReturnsAsync(IdentityResultAlias.Success);

            // Act: Call the DeleteUserConfirmed method with a valid user ID
            var result = await _controller.DeleteUserConfirmed("1") as RedirectToActionResult;

            // Debug: Check what is returned by FindByIdAsync
            var foundUser = await _userRepositoryMock.Object.FindByIdAsync("1");
            Debug.WriteLine(foundUser == null ? "User is null" : $"User: {foundUser.UserName}");

            // Assert: Verify redirection and repository calls
            Assert.NotNull(result);
            Assert.Equal("UserManager", result?.ActionName);

            _userRepositoryMock.Verify(repo => repo.FindByIdAsync("1"), Times.Once);
            _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(It.Is<IdentityUser>(u => u.Id == "1")), Times.Once);
        }

        // Test: DeleteUserConfirmed - Verify that an invalid user ID returns NotFound
        [Fact]
        public async Task DeleteUserConfirmed_InvalidUser_ReturnsNotFound()
        {
            // Arrange: Mock FindByIdAsync to return null
            _userRepositoryMock.Setup(repo => repo.FindByIdAsync("invalid-id"))
                .ReturnsAsync((IdentityUser)null);

            // Act: Call the DeleteUserConfirmed method with an invalid user ID
            var result = await _controller.DeleteUserConfirmed("invalid-id");

            // Assert: Verify that NotFound is returned
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
