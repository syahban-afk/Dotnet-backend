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
        public async Task<IActionResult> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            // Cek apakah user ingin pagination atau semua data
            // Jika page dan pageSize adalah 0 atau nilai default lainnya, asumsikan ingin semua data
            bool isPaginationEnabled = page > 0 && pageSize > 0;

            IQueryable<Product> query = _context.Product.Include(p => p.Category);

            // Jika pagination diaktifkan
            if (isPaginationEnabled)
            {
                var skipCount = (page - 1) * pageSize;
                var products = await query
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToListAsync();

                var totalCount = await _context.Product.CountAsync();

                var response = new
                {
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = products
                };
                return Ok(response);
            }
            else // Jika tidak ada parameter pagination, kembalikan semua data
            {
                var allProducts = await query.ToListAsync();
                return Ok(allProducts);
            }
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
        public async Task<IActionResult> Create([FromForm] Product product, IFormFile? image)
        {
            // Cek validasi model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifikasi CategoryId
            var category = await _context.ProductCategories.FindAsync(product.CategoryId);
            if (category == null)
            {
                return BadRequest(new { Error = "The Category field is required.", Message = $"Category with ID {product.CategoryId} not found." });
            }

            // Jika ada file gambar di-upload
            if (image != null && image.Length > 0)
            {
                // 1. Simpan gambar ke folder wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine("wwwroot", "images", fileName);

                // Pastikan direktori ada
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // 2. Simpan path gambar di model
                product.ImageUrl = "/images/" + fileName;
            }
            // Jika tidak ada gambar, ImageUrl akan tetap null (jika kamu set nullable)

            // Set Category property
            product.Category = category;

            // Tambahkan produk ke database
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
        public async Task<IActionResult> Update(Guid id, [FromBody] Product updated, IFormFile? image = null)
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

            // Cek apakah ada file gambar baru yang diunggah
            if (image != null && image.Length > 0)
            {
                // Hapus gambar lama jika ada
                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    var oldFilePath = Path.Combine("wwwroot", existing.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Simpan gambar baru
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine("wwwroot", "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Simpan path gambar baru ke model
                existing.ImageUrl = "/images/" + fileName;
            }

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
