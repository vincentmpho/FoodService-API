using FoodService_API.Data;
using FoodService_API.Models;
using FoodService_API.Models.DTOs;
using FoodService_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoodService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string _secretKey;

        public AuthController(ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if the username already exists
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    return Conflict("Username already exists.");
                }

                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.UserName,
                    NormalizedEmail = model.UserName.ToUpper(),
                    Name = model.Name
                };

                var createUserResult = await _userManager.CreateAsync(newUser, model.Password);

                if (!createUserResult.Succeeded)
                {
                    var errorMessages = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    return BadRequest($"Error while registering user: {errorMessages}");
                }

                // Ensure roles exist
                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    //Creates Roles in Database
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                }

                // Assign role to user
                var role = model.Role.ToLower() == SD.Role_Admin ? SD.Role_Admin : SD.Role_Customer;
                await _userManager.AddToRoleAsync(newUser, role);

                return Ok("Registration successful.");
            }
            catch (Exception ex)
            {
             
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
             ApplicationUser userFromDb = _context.ApplicationUsers
                .FirstOrDefault(U => U.UserName.ToLower()==model.UserNmae.ToLower());

            bool isValid = await  _userManager.CheckPasswordAsync(userFromDb, model.Password);

            if (isValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "UserName or Password is incorrect");
            }

            //Generate JWT Token
            var roles = await _userManager.GetRolesAsync(userFromDb);
            JwtSecurityTokenHandler tokenHandler = new();
            Byte[] key = Encoding.ASCII.GetBytes(_secretKey);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim ("fullName", userFromDb.Name ),
                    new Claim ("Id",userFromDb.Id.ToString()),
                    new Claim (ClaimTypes.Email, userFromDb.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                }),

                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);



            LoginResponseDto loginResponse = new()   

            {
               Email = userFromDb.Email,
               Token =tokenHandler.WriteToken(token)
            };

            if (loginResponse.Email == null || string.IsNullOrEmpty(loginResponse.Token)) 
            {
                return StatusCode(StatusCodes.Status400BadRequest, "UserName or Password is incorrect");
            }

            return StatusCode(StatusCodes.Status200OK, loginResponse);
        }
    }
}
