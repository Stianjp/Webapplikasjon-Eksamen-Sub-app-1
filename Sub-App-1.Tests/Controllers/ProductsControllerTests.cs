using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Sub_App_1.Controllers;
using Sub_App_1.DAL.Interfaces;
using Sub_App_1.Models;
using Sub_App_1.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;



namespace Sub_App_1.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            // Initialize a mock for IProductRepository
            _productRepositoryMock = new Mock<IProductRepository>();

            // Initialize ProductsController with the mocked repository
            _controller = new ProductsController(_productRepositoryMock.Object);

            // Add support for TempData in the controller
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;
        }

        // Test: Productsindex - Checks if the method returns a list of products
        [Fact]
        public async Task Productsindex_ReturnsViewWithProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Category = "Fruit" },
                new Product { Id = 2, Name = "Product2", Category = "Meat" }
            };

            _productRepositoryMock
                .Setup(repo => repo.GetAllProductsAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _controller.Productsindex() as ViewResult;

            // Assert
            Assert.NotNull(result); // Ensure a ViewResult is returned
            var model = result.Model as List<Product>;
            Assert.NotNull(model); // Verify the model is correct
            Assert.Equal(2, model.Count); // Ensure it contains two products
        }

        // Test: Details - Returns NotFound if the product does not exist
        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _productRepositoryMock
                .Setup(repo => repo.GetProductByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.Details(99);

            // Assert
            Assert.IsType<NotFoundResult>(result); // Ensure NotFound is returned
        }

        // Test: Create (POST) - Redirects if the model is valid
        [Fact]
public async Task Create_ValidProduct_RedirectsToProductsindex()
        {
            // Arrange
            var viewModel = new ProductFormViewModel
            {
                Name = "NewProduct",
                ProducerId = "Producer123",
                CategoryList = new List<string> { "Fruit" }
            };

            _productRepositoryMock
                .Setup(repo => repo.CreateProductAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Mock User
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "Producer123")
            };
            var identity = new ClaimsIdentity(claims, "mock");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Ensure ModelState is valid
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Create(viewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Productsindex", result.ActionName);
        }





        // Test: Create (POST) - Returns the view with an error if the model is invalid
        [Fact]
        public async Task Create_InvalidModel_ReturnsViewWithError()
        {
            // Arrange
            var viewModel = new ProductFormViewModel
            {
                Name = "NewProduct",
                //ProducerId = null, // Invalid ProducerId
                //CategoryList = null // Missing category
                ProducerId = string.Empty, // Simulate invalid ProducerId
                CategoryList = new List<string>()
            };

            _controller.ModelState.AddModelError("ProducerId", "ProducerId is required.");

            // Act
            var result = await _controller.Create(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductFormViewModel>(result.Model);
            Assert.True(_controller.ModelState.ContainsKey("ProducerId"));
        }


        // Test: DeleteConfirmed - Redirects if the product is successfully deleted
        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToProductsindex()
        {
            // Arrange

            var product = new Product { Id = 1, ProducerId = "Producer123" };

            _productRepositoryMock.Setup(repo => repo.GetProductByIdAsync(1)).ReturnsAsync(product);

            _productRepositoryMock.Setup(repo => repo.DeleteProductAsync(1)).ReturnsAsync(true);

            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true); // Simulate admin user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            // Act
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Productsindex", result.ActionName);
        }


        // Test: DeleteConfirmed - Returns NotFound if the product does not exist
        [Fact]
        public async Task DeleteConfirmed_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _productRepositoryMock
                .Setup(repo => repo.DeleteProductAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteConfirmed(99);

            // Assert
            Assert.IsType<NotFoundResult>(result); // Ensure NotFound is returned
        }


        // Test: Edit (GET) - Returns the product if it exists
        [Fact]
        public async Task Edit_ValidId_ReturnsViewWithProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product1", Category = "Fruit" };

            _productRepositoryMock
                .Setup(repo => repo.GetProductByIdAsync(1))
                .ReturnsAsync(product);

            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(false); // Simulate non-admin user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as ProductFormViewModel;
            Assert.NotNull(model);
            Assert.Equal("Product1", model.Name);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewWithErrors()
        {
            // Arrange
            var viewModel = new ProductFormViewModel
            {
                Id = 1,
                Name = "UpdatedProduct",
                ProducerId = "Producer123",
                CategoryList = new List<string>() // Simulate invalid state
            };

            // Add error to ModelState
            _controller.ModelState.AddModelError("CategoryList", "Please select at least one category.");

            // Mock the product to simulate repository behavior
            Product? product = new Product
            {
                Id = 1,
                Name = "Product1",
                ProducerId = "Producer123",
                CategoryList = new List<string> { "Fruit" }
            };

            _productRepositoryMock.Setup(repo => repo.GetProductByIdAsync(1)).ReturnsAsync(product);

            // Mock User
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "Producer123")
            };
            var identity = new ClaimsIdentity(claims, "mock");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.Edit(1, viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result); // Ensure ViewResult is returned
            var returnedViewModel = result?.Model as ProductFormViewModel;
            Assert.NotNull(returnedViewModel); // Ensure the ViewModel is returned
            Assert.True(_controller.ModelState.ContainsKey("CategoryList")); // Ensure the error is in ModelState
            Assert.Equal("Please select at least one category.", _controller.ModelState["CategoryList"]!.Errors[0].ErrorMessage); // Validate the error message
            Assert.NotNull(result?.ViewData["AllergenOptions"]); // Ensure ViewBag.AllergenOptions is set
            Assert.NotNull(result?.ViewData["CategoryOptions"]); // Ensure ViewBag.CategoryOptions is set
        }
    }
}
