using LidomNet.Api.Configuration;
using LidomNet.Data.Auth;
using LidomNet.Data.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LidomNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public Authentication(UserManager<IdentityUser> userManager, IOptions<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            // Verificar si el email existe
            var emailExists = await _userManager.FindByEmailAsync(request.EmailAddress);

            if (emailExists != null)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = ["El Email ya existe"]
                });
            }

            // Si el email no existe, creamos el usuario
            var user = new IdentityUser()
            {
                Email = request.EmailAddress,
                UserName = request.Name
            };

            var isCreated = await _userManager.CreateAsync(user, request.Password);

            if (isCreated.Succeeded)
            {
                var token = GenerateToken(user);

                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = token
                });
            }
            else
            {
                var errors = new List<string>();

                foreach (var err in isCreated.Errors) errors.Add(err.Description);
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = errors
                });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            // Check if user exist via Email.
            var existingUser = await _userManager.FindByEmailAsync(request.EmailAddress);

            if(existingUser == null)
            {
                return BadRequest(new AuthResult() { Result = false, Errors = ["Invalid Payload"] });
            }

            var checkUserPass = await _userManager.CheckPasswordAsync(existingUser, request.Password);

            if (!checkUserPass) return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = ["Incorrect Password!, Try Again!"]
            });

            var token = GenerateToken(existingUser);

            return Ok(new AuthResult(){ Result = true, Token = token });
        }

        private string GenerateToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new ClaimsIdentity(
                [
                    new ("Id", user.Id),
                    new (JwtRegisteredClaimNames.Sub, user.UserName!),
                    new (JwtRegisteredClaimNames.Email, user.Email!),
                    new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new (JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),

                ]
                )),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }
    }
}
