using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Sub_App_1.Controllers;
using Sub_App_1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sub_App_1.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            // Bruk tomme mock-objekter istedenfor null
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

            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object, 
                new IRoleValidator<IdentityRole>[0], 
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object
            );

            // Initialiser AdminController med mockede objekter
            _controller = new AdminController(_userManagerMock.Object, _roleManagerMock.Object);
        }

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

            _userManagerMock.Setup(um => um.Users).Returns(users.AsQueryable());
            _userManagerMock.Setup(um => um.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "1"))).ReturnsAsync(roles1);
            _userManagerMock.Setup(um => um.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "2"))).ReturnsAsync(roles2);

            // Act
            var result = await _controller.UserManager() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<UserWithRolesViewModel>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
            Assert.Equal("user1", model[0].Username);
            Assert.Equal("Role1", model[0].Roles[0]);
        }

        [Fact]
        public async Task EditUser_ValidUserId_ReturnsViewWithUserRoles()
        {
            // Arrange
            var user = new IdentityUser { Id = "1", UserName = "testuser" };
            var roles = new List<string> { "Role1", "Role2" };
            var allRoles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Role1" },
                new IdentityRole { Name = "Role2" },
                new IdentityRole { Name = "Role3" }
            };

            _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(allRoles.AsQueryable());

            // Act
            var result = await _controller.EditUser("1") as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as UserWithRolesViewModel;
            Assert.NotNull(model);
            Assert.Equal("testuser", model.Username);
            Assert.Equal(2, model.Roles.Count);
        }

        [Fact]
        public async Task EditUser_InvalidUserId_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("invalid-id")).ReturnsAsync((IdentityUser)null!);

            // Act
            var result = await _controller.EditUser("invalid-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
public async Task DeleteUserConfirmed_ValidUser_RedirectsToUserManager()
{
    // Arrange
    var userId = "1";
    var user = new IdentityUser { Id = userId };

    // Set up TempData
    _controller.TempData = new Mock<ITempDataDictionary>().Object;

    _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
    _userManagerMock.Setup(um => um.DeleteAsync(user)).Returns(Task.FromResult(IdentityResult.Success));


    // Act
    var result = await _controller.DeleteUserConfirmed(userId) as RedirectToActionResult;

    // Assert
    Assert.NotNull(result);
    Assert.Equal("UserManager", result.ActionName);
    _userManagerMock.Verify(um => um.DeleteAsync(It.Is<IdentityUser>(u => u.Id == userId)), Times.Once);
}

        [Fact]
        public async Task DeleteUserConfirmed_InvalidUser_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("invalid-id")).ReturnsAsync((IdentityUser)null!);

            // Act: Call the method with a invalid ID
            var result = await _controller.DeleteUserConfirmed("invalid-id");

            // Assert: Check that the result is NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        
        public async Task DeleteUserConfirmed_ValidUser_DeleteFails_ReturnsErrorInTempData()
        {
            // Arrange
            var userId = "1";
            var user = new IdentityUser { Id = userId };

            // Set up TempData using a mock TempDataDictionary
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Set up mock to return user but delete fails
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.DeleteAsync(user)).Returns(Task.FromResult(IdentityResult.Failed()));


            // Act
            var result = await _controller.DeleteUserConfirmed(userId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UserManager", result.ActionName);

            // Verify that TempData["Error"] was set with the expected error message
            tempDataMock.VerifySet(tempData => tempData["Error"] = "An unexpected error occurred.");
}       
    }
}
