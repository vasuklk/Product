using Microsoft.AspNetCore.Mvc;
using ProductAPI.Models;
using ProductAPI.Repositories;
using Microsoft.Extensions.Logging;
using ProductAPI.Helpers;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductsController> _logger;
        private readonly IUniqueIdGenerator _idGenerator;

        public ProductsController(
            IProductRepository repository,
            ILogger<ProductsController> logger,
            IUniqueIdGenerator idGenerator)
        {
            _repository = repository;
            _logger = logger;
            _idGenerator = idGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Getting all products");
            var products = await _repository.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation("Getting product with Id: {Id}", id);
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id: {Id} not found", id);
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto dto)
        {
            _logger.LogInformation("Creating a new product");

            int newId = await _idGenerator.GenerateUniqueIdAsync();

            var product = new Product
            {
                Id = newId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Quantity = dto.Quantity
            };
            await _repository.AddAsync(product);
            _logger.LogInformation("Product created with Id: {Id}", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
        {
            _logger.LogInformation("Updating product with Id: {Id}", id);
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id: {Id} not found for update", id);
                return NotFound();
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;

            await _repository.UpdateAsync(product);
            _logger.LogInformation("Product with Id: {Id} updated", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with Id: {Id}", id);
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id: {Id} not found for deletion", id);
                return NotFound();
            }

            await _repository.DeleteAsync(product);
            _logger.LogInformation("Product with Id: {Id} deleted", id);
            return NoContent();
        }

        [HttpPut("decrement-stock/{id}/{quantity}")]
        public async Task<ActionResult<Product>> DecrementStock(int id, int quantity)
        {
            _logger.LogInformation("Decrementing stock for product Id: {Id} by {Quantity}", id, quantity);
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id: {Id} not found for stock decrement", id);
                return NotFound();
            }

            if (product.Quantity < quantity)
            {
                _logger.LogWarning("Insufficient stock for product Id: {Id}. Requested: {Requested}, Available: {Available}", id, quantity, product.Quantity);
                return BadRequest($"Insufficient stock. Remaining stock is {product.Quantity}.");
            }

            product.Quantity -= quantity;
            await _repository.UpdateAsync(product);

            _logger.LogInformation("Stock decremented for product Id: {Id}. New quantity: {Quantity}", id, product.Quantity);
            return Ok(product);
        }

        [HttpPut("increment-stock/{id}/{quantity}")]
        public async Task<ActionResult<Product>> IncrementStock(int id, int quantity)
        {
            _logger.LogInformation("Incrementing stock for product Id: {Id} by {Quantity}", id, quantity);
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id: {Id} not found for stock increment", id);
                return NotFound();
            }

            product.Quantity += quantity;
            await _repository.UpdateAsync(product);

            _logger.LogInformation("Stock incremented for product Id: {Id}. New quantity: {Quantity}", id, product.Quantity);
            return Ok(product);
        }
    }
}