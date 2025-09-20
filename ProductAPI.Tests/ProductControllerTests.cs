using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ProductAPI.Controllers;
using ProductAPI.Repositories;
using ProductAPI.Models;
using ProductAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ProductAPI.Tests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();
        private readonly Mock<ILogger<ProductsController>> _loggerMock = new();
        private readonly Mock<IUniqueIdGenerator> _idGenMock = new();

        private ProductsController CreateController()
        {
            return new ProductsController(_repoMock.Object, _loggerMock.Object, _idGenMock.Object);
        }

        [Fact]
        public async Task GetProducts_ReturnsOk_WithProducts()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> { new Product { Id = 1 } });
            var controller = CreateController();

            var result = await controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
            Assert.Single(products);
        }

        [Fact]
        public async Task GetProduct_ReturnsOk_WhenFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Product { Id = 1 });
            var controller = CreateController();

            var result = await controller.GetProduct(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(1, product.Id);
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Product?)null);
            var controller = CreateController();

            var result = await controller.GetProduct(2);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreated_WithProduct()
        {
            var dto = new ProductCreateDto { Name = "Test", Description = "Desc", Price = 10, Quantity = 5 };
            _idGenMock.Setup(g => g.GenerateUniqueIdAsync()).ReturnsAsync(428674);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);
            _idGenMock.Setup(g => g.IsValidId(428674)).Returns(true);
            var controller = CreateController();

            var result = await controller.CreateProduct(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var product = Assert.IsType<Product>(created.Value);
            Assert.Equal(428674, product.Id);
            Assert.Equal("Test", product.Name);
            Assert.True(product.Id >= 100000 && product.Id < 1000000);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsNoContent_WhenFound()
        {
            var dto = new ProductCreateDto { Name = "Updated", Description = "Desc", Price = 20, Quantity = 10 };
            var product = new Product { Id = 123456, Name = "Old", Description = "Old", Price = 5, Quantity = 2 };
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync(product);
            _repoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.UpdateProduct(123456, dto);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("Updated", product.Name);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync((Product?)null);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.UpdateProduct(123456, new ProductCreateDto());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenFound()
        {
            var product = new Product { Id = 123456 };
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync(product);
            _repoMock.Setup(r => r.DeleteAsync(product)).Returns(Task.CompletedTask);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.DeleteProduct(123456);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync((Product?)null);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.DeleteProduct(123456);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DecrementStock_ReturnsOk_WhenSufficientStock()
        {
            var product = new Product { Id = 1, Quantity = 10 };
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync(product);
            _repoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.DecrementStock(123456, 5);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updated = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(5, updated.Quantity);
        }

        [Fact]
        public async Task DecrementStock_ReturnsBadRequest_WhenInsufficientStock()
        {
            var product = new Product { Id = 123456, Quantity = 2 };
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync(product);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.DecrementStock(123456, 5);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Insufficient stock", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task DecrementStock_ReturnsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync((Product?)null);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.DecrementStock(123456, 1);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task IncrementStock_ReturnsOk_WhenFound()
        {
            var product = new Product { Id = 123456, Quantity = 2 };
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync(product);
            _repoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.IncrementStock(123456, 3);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updated = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(5, updated.Quantity);
        }

        [Fact]
        public async Task IncrementStock_ReturnsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(123456)).ReturnsAsync((Product?)null);
            _idGenMock.Setup(g => g.IsValidId(123456)).Returns(true);
            var controller = CreateController();

            var result = await controller.IncrementStock(123456, 1);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}