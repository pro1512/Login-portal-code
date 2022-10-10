using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectCTS.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjectCTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterDetailsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public RegisterDetailsController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("Display")]
        public async Task<ActionResult<List<Register>>> getRegister()
        {
            return Ok(await _context.registers.ToListAsync());
        }
        [HttpPost("EnterDetail")]
        public async Task<ActionResult<List<Register>>> CreateRegister(Register register)
        {
            _context.registers.Add(register);
            await _context.SaveChangesAsync();

            return Ok(await _context.registers.ToListAsync());
        }

        [HttpDelete("Delete")]

        public async Task<ActionResult<List<Register>>> DeleteRegister(int id)
        {
            var dbmember = await _context.registers.FindAsync(id);
            if(dbmember == null)
            {
                return BadRequest("Data not found");
            }
            _context.registers.Remove(dbmember);
            await _context.SaveChangesAsync();

            return Ok(await _context.registers.ToListAsync());
        }



        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(Register request)
        {
            if (request == null)
                return BadRequest("Hero not FFound");
            var dbHero = await _context.registers.SingleOrDefaultAsync(x => x.Email.Equals(request.Email));

            if (dbHero?.Password == request.Password)
            {
                string token = CreateToken(request.Email);
                return Ok(token);
            }

            return BadRequest("Wrong Password");
        }

        private string CreateToken(string email)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("JWT:Token").Value));

            var signinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    issuer: _configuration.GetSection("JWT:ValidIssuer").Value,
                    audience: _configuration.GetSection("JWT:ValidAudience").Value,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: signinCredentials
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpGet, Authorize]
        public ActionResult<object> GetMe()
        {
            // var userName = User?.Identity?.Name;
            var email = User?.FindFirstValue(ClaimTypes.Email);
            var role = User?.FindFirstValue(ClaimTypes.Role);
            return Ok(new { email, role });
        }
    }
}
