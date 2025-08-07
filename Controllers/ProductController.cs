using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            return await _context.Product
                .Include(p => p.Category) // include relasi category
                .ToListAsync();
        }

        // GET: api/Product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(Guid id)
        {
            var product = await _context.Product
                .Include(p => p.Category) // include category
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            return product;
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verifikasi bahwa CategoryId valid
            var category = await _context.ProductCategories.FindAsync(product.CategoryId);
            if (category == null)
            {
                return BadRequest(new { Error = "The Category field is required.", Message = $"Category with ID {product.CategoryId} not found." });
            }

            // Set Category property
            product.Category = category;

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                new { Message = "Product successfully created.", Data = product }
            );
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Product updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Product.FindAsync(id);

            if (existing == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            existing.Name = updated.Name;
            existing.Price = updated.Price;
            existing.Qty = updated.Qty;
            existing.Description = updated.Description;
            existing.CategoryId = updated.CategoryId;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Product successfully updated.", Data = existing });
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Product successfully deleted." });
        }
    }
}
