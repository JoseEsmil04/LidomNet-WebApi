using LidomNet.Api.Configuration;
using LidomNet.Data;
using LidomNet.Data.Auth;
using LidomNet.Data.Common;
using LidomNet.Data.DTOs;
using LidomNet.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace LidomNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailSender _emailSender;
        private readonly LidomNetDbContext _dbContext;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationController(
            UserManager<IdentityUser> userManager,
            IOptions<JwtConfig> jwtConfig,
            IEmailSender emailSender,
            LidomNetDbContext dbContext,
            TokenValidationParameters tokenValidationParameters
        )
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _tokenValidationParameters = tokenValidationParameters;
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
                UserName = request.Name,
                EmailConfirmed = false
            };

            var isCreated = await _userManager.CreateAsync(user, request.Password);

            if (isCreated.Succeeded)
            {
                // var token = GenerateToken(user);
                await SendVerificationEmail(user);

                return Ok(new AuthResult()
                {
                    Result = true
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

            if(!existingUser.EmailConfirmed)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = ["Email needs to be confirmed!"]
                });
            }

            var checkUserPass = await _userManager.CheckPasswordAsync(existingUser, request.Password);

            if (!checkUserPass) return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = ["Incorrect Password!, Try Again!"]
            });

            var token = GenerateToken(existingUser);

            return Ok(token);
        }

        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if(!ModelState.IsValid)
                return BadRequest(new AuthResult
                {
                    Errors = ["Invalid Parameters!"],
                    Result = false
                });

            var results = await VerifyAndGenerateTokenAsync(tokenRequest);

            if(results == null)
                return BadRequest(new AuthResult
                {
                    Errors = ["Invalid Parameters!"],
                    Result = false
                });

            return Ok(results);

        }

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
                return BadRequest(new AuthResult()
                {
                    Errors = ["Invalid email confirmation url"],
                    Result = false
                });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound($"Unable to load user with id {userId}");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            var status = result.Succeeded ? "Thank you for confirming your email!" : "There has been an error confirmig your email";

            return Ok(status);
        }

        private async Task<AuthResult> GenerateToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret!);

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
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            RefreshToken refreshToken = new()
            {
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                JwtId = token.Id,
                Token = RandomGenerator.GenerateRandomString(24),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id
            };

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new AuthResult() { RefreshToken = refreshToken.Token, Result = true, Token = jwtToken };
        }

        private async Task SendVerificationEmail(IdentityUser user)
        {
            var verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationCode));

            //example: https://localhost:8080/api/authentication/verifyemail/userId=exampleuserId&code=examplecode
            var callbackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
                new { userId = user.Id, code = verificationCode })}";

            var emailBody = $"Please, confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here!</a>";

            await _emailSender.SendEmailAsync(user.Email!, "Confirm your email", emailBody);
        }

        private async Task<AuthResult> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;

                var tokenBeingVerified = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                        StringComparison.InvariantCultureIgnoreCase);

                    if (!result || tokenBeingVerified == null)
                        throw new Exception("Invalid Token");
                }

                var utcExpiryDate = long.Parse(tokenBeingVerified.Claims.
                                        FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)!.Value);

                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;
                if (expiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                var storedToken = await _dbContext.RefreshTokens.
                        FirstOrDefaultAsync(t => t.Token == tokenRequest.RefreshToken);
                if (storedToken == null)
                    throw new Exception("Invalid Token");

                if (storedToken.IsUsed || storedToken.IsRevoked)
                    throw new Exception("Invalid Token");

                var jti = tokenBeingVerified.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)!.Value;

                if (jti != storedToken.JwtId)
                    throw new Exception("Invalid Token");

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                storedToken.IsUsed = true;
                _dbContext.RefreshTokens.Update(storedToken);
                await _dbContext.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);

                return await GenerateToken(dbUser!);
            }
            catch (Exception e)
            {
                var message = e.Message == "Invalid Token" || e.Message == "Token Expired"
                    ? e.Message
                    : "Internal Server Error";
                return new AuthResult() { Result = false, Errors = new List<string> { message } };
            }
        }
    }
}
