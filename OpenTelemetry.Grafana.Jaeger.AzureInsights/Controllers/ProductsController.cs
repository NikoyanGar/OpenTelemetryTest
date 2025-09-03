using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryTest.Api.Data;
using System.Diagnostics;
using System.Threading.Tasks;
namespace OpenTelemetryTest.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsDataContext _dataContext;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductsDataContext dataContext, ILogger<ProductsController> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Getting all products");
            var products = await _dataContext.Products.ToListAsync();
            _logger.LogInformation("Retrieved {Count} products", products.Count);
            return products;
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation("Getting product with id {Id}", id);
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with id {Id} not found", id);
                return NotFound();
            }
            _logger.LogInformation("Product with id {Id} retrieved", id);
            return product;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _logger.LogInformation("Creating new product: {@Product}", product);
            _dataContext.Products.Add(product);
            await _dataContext.SaveChangesAsync();
            _logger.LogInformation("Product created with id {Id}", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                _logger.LogWarning("Product id mismatch: route id {RouteId}, body id {BodyId}", id, product.Id);
                return BadRequest();
            }

            _dataContext.Entry(product).State = EntityState.Modified;

            try
            {
                await _dataContext.SaveChangesAsync();
                _logger.LogInformation("Product with id {Id} updated", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(id))
                {
                    _logger.LogWarning("Product with id {Id} not found for update", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency exception when updating product with id {Id}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with id {Id}", id);
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with id {Id} not found for deletion", id);
                return NotFound();
            }

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            _logger.LogInformation("Product with id {Id} deleted", id);

            return NoContent();
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _dataContext.Products.AnyAsync(e => e.Id == id);
        }
    }
}
