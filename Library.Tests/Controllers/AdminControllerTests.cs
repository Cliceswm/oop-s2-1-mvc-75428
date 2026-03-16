using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Library.MVC.Controllers;

namespace Library.Tests.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public void AdminController_RequiresAdminRole()
        {
            // Arrange
            var controllerType = typeof(AdminController);

            // Act
            var attribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Contains("Admin", attribute.Roles);
        }

        [Fact]
        public void RolesAction_ReturnsViewResult()
        {
            // Arrange
            var controllerType = typeof(AdminController);
            var method = controllerType.GetMethod("Roles");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(IActionResult), method.ReturnType);
        }
    }
}