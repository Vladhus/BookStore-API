using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStore_API.Contracts;
using BookStore_API.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILoggerService _loggerService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _Config;

        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILoggerService loggerService, IConfiguration Config)
        {
            _Config = Config;
            _loggerService = loggerService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// User Login EndPoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                var username = userDTO.Username;
                var password = userDTO.Password;

                _loggerService.LogInfo($"{location}: Login Attemped from user {username} ");

                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    _loggerService.LogInfo($"{location}: {username} Successfully Authenticated ");

                    var user = await _userManager.FindByNameAsync(username);
                    var tokenString = await GenerateJSONWebToken(user);

                    return Ok(new { token = tokenString });
                }

                _loggerService.LogInfo($"{location}: {username} Not Authenticated ");

                return Unauthorized(userDTO);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Config["Jwt:Key"]));//KEY GANERATING
            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);//HASH
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Email), //CLAIM "Subject of the JWT (the user)"
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()), //CLAIM IDENTITY "Unique identifier; can be used to prevent the JWT from being replayed
                                                                                  //(allows a token to be used only once)"
                new Claim(ClaimTypes.NameIdentifier,user.Id) //CLAIM USER_ID
            };

            var roles = await _userManager.GetRolesAsync(user); //Get user Roles 
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r))); //Add new Claim 

            var token = new JwtSecurityToken(_Config["Jwt:Issuer"], //TOKEN GENERATE
                _Config["Jwt:Issuer"],
                claims,
                null,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong.Please contact the Administrator");
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
    }
}
