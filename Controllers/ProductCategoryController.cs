using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductCategory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetAll()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        // GET: api/ProductCategory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCategory>> GetById(Guid id)
        {
            var productCategory = await _context.ProductCategories.FindAsync(id);

            if (productCategory == null)
            {
                return NotFound(new { Message = $"Category with ID {id} not found." });
            }

            return productCategory;
        }

        // POST: api/ProductCategory
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCategory productCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = productCategory.Id },
                new { Message = "Category successfully created.", Data = productCategory }
            );
        }

        // PUT: api/ProductCategory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductCategory updatedCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCategory = await _context.ProductCategories.FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound(new { Message = $"Category with ID {id} not found." });
            }

            existingCategory.Name = updatedCategory.Name;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Category successfully updated.", Data = existingCategory });
        }
        
                // DELETE: api/ProductCategory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var productCategory = await _context.ProductCategories.FindAsync(id);
 
            if (productCategory == null)
            {
                return NotFound(new { Message = $"Category with ID {id} not found." });
            }
 
            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
 
            return Ok(new { Message = "Category successfully deleted." });
        }
    }

}