using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using WebApi.Service;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public static User user = new User();

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto userDto)
        {
            user.Username = userDto.Username;
            user.Password = userDto.Password;
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto userDto)
        {
            if (userDto.Username != user.Username)
            {
                return BadRequest("tai khoan khong ton tai");
            }
            if (!VerifyPassword(userDto))
            {
                return BadRequest("sai mat khau");
            }
            string token = CreateToken(user);
            return token;
            
        }

        [HttpGet,Authorize]
        public ActionResult<string> GetInfo()
        {
            var userName = _userService.getName();
            return userName;
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "noob")
            };
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials:cred
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        private bool VerifyPassword(UserDto userDto)
        {
            if (userDto.Password != user.Password)
            {
                return false;
            }

            return true;
        }


    }
}
