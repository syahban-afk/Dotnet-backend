using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Cek apakah user sudah terdaftar
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Telephone == dto.Telephone);
            if (existingCustomer != null)
            {
                return BadRequest(new { message = "User with this phone number already exists." });
            }

            // 2. Hash password sebelum disimpan
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Buat objek Customer baru
            var newCustomer = new Customer
            {
                Name = dto.Name,
                Telephone = dto.Telephone,
                PasswordHash = passwordHash
            };

            // 4. Simpan ke database
            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = newCustomer.Id },
                new { Message = "Registration successfully", Data = newCustomer }
            );
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Cari user berdasarkan nomor telepon
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Telephone == dto.Telephone);
            if (customer == null)
            {
                return Unauthorized(new { message = "Invalid phone number or password." });
            }

            // 2. Verifikasi password yang diinput dengan hash yang tersimpan
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid phone number or password." });
            }

            // 1. Buat Access Token
            var accessToken = GenerateJwtToken(customer);

            // 2. Buat Refresh Token
            var refreshToken = GenerateRefreshToken();
            customer.RefreshToken = refreshToken;
            customer.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1); // Masa berlaku 7 hari
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return Ok(new { accessToken, refreshToken });
        }

        private string GenerateJwtToken(Customer customer)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
#pragma warning restore CS8604 // Possible null reference argument.
            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

#pragma warning disable CS8604 // Possible null reference argument.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customer.Telephone),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", customer.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDto tokenDto)
        {
            // 1. Validasi Refresh Token
            var customer = await _context.Customers.SingleOrDefaultAsync(c => c.RefreshToken == tokenDto.RefreshToken);

            // Periksa apakah refresh token valid dan belum kedaluwarsa
            if (customer == null || customer.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired refresh token.");
            }

            // 2. Buat Access Token dan Refresh Token baru
            var newAccessToken = GenerateJwtToken(customer);
            var newRefreshToken = GenerateRefreshToken();

            // 3. Simpan Refresh Token baru ke database
            customer.RefreshToken = newRefreshToken;
            customer.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(Guid id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(p => p.Id == id);

            if (customer == null)
            {
                return NotFound(new { Message = $"Customer with ID {id} not found." });
            }

            return customer;
        }
    }

    public class TokenDto
    {
        public string? RefreshToken { get; internal set; }
    }
}