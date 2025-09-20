using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductAPI.Data;
using ProductAPI.Models;
using ProductAPI.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProductAPI.Tests
{
    public class ProductRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private ProductRepository CreateRepository(AppDbContext context)
        {
            var loggerMock = new Mock<ILogger<ProductRepository>>();
            return new ProductRepository(context, loggerMock.Object);
        }

        [Fact]
        public async Task AddAsync_AddsProduct()
        {
            var context = GetDbContext();
            var repo = CreateRepository(context);
            var product = new Product { Id = 123456, Name = "Test", Description = "Desc", Price = 10, Quantity = 5 };

            var result = await repo.AddAsync(product);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
            Assert.Single(context.Products);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { Id = 123456, Name = "A", Description = "Desc", Price = 10, Quantity = 5 });
            context.Products.Add(new Product { Id = 234567, Name = "B", Description = "Desc", Price = 10, Quantity = 5 });
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repo = CreateRepository(context);

            var products = await repo.GetAllAsync();

            Assert.Equal(2, System.Linq.Enumerable.Count(products));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProduct_WhenExists()
        {
            var context = GetDbContext();
            var product = new Product { Name = "FindMe", Description = "Desc", Price = 10, Quantity = 5 };
            context.Products.Add(product);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repo = CreateRepository(context);

            var found = await repo.GetByIdAsync(product.Id);

            Assert.NotNull(found);
            Assert.Equal("FindMe", found.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            var context = GetDbContext();
            var repo = CreateRepository(context);

            var found = await repo.GetByIdAsync(999999);

            Assert.Null(found);
        }

        [Fact]
        public async Task AddAsync_ReturnsProduct()
        {
            var context = GetDbContext();
            var product = new Product { Id = 987513, Name = "Product", Description = "Product Description", Price = 1, Quantity = 1 };

            var repo = CreateRepository(context);

            var updated = await repo.AddAsync(product);
            Assert.Equal(product.Name, updated.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesProduct()
        {
            var context = GetDbContext();
            var product = new Product { Name = "Old", Description = "OldDesc", Price = 1, Quantity = 1 };
            context.Products.Add(product);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repo = CreateRepository(context);

            product.Name = "New";
            await repo.UpdateAsync(product);

            var updated = await repo.GetByIdAsync(product.Id);
            Assert.Equal("New", updated?.Name);
        }

        [Fact]
        public async Task DeleteAsync_RemovesProduct()
        {
            var context = GetDbContext();
            var product = new Product { Name = "DeleteMe", Description = "Desc", Price = 10, Quantity = 5 };
            context.Products.Add(product);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repo = CreateRepository(context);

            await repo.DeleteAsync(product);

            var found = await repo.GetByIdAsync(product.Id);
            Assert.Null(found);
        }
    }
}