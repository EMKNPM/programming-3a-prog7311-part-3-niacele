using GLMS.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
     
        private readonly GLMSContext _context;

        public AuthController(GLMSContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // find the matching credentials in the database
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == request.Username && u.Password == request.Password);

            // if a user is found, generate a token
            if (user != null)
            {
                var jwtKey = "GLMS_Super_Secret_Security_Key_2026_Do_Not_Share";
                var tokenHandler = new JwtSecurityTokenHandler();
                var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    // attach a token to the relevant employee
                    Subject = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // token goes back to the front end
                return Ok(new { Token = tokenString, FullName = user.FullName, Role = user.Role });
            }

            // reject if unauthorized user tries to login
            return Unauthorized(new { message = "Authentication failed: Invalid internal credentials." });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}