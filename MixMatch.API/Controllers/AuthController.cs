using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MixMatch.API.Data;
using MixMatch.API.Dtos;
using MixMatch.API.Models;

namespace MixMatch.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController :ControllerBase
    {
        private readonly IAuthRepository _repo;

        public IConfiguration _config { get; }

        public AuthController(IAuthRepository repo,IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registor (UserForRegistorDto userForRegistor)
        {
            //validate request
            //this is required if [ApiController] decorator is removed.
            //if(!ModelState.IsValid)
            //    return BadRequest("Username and password can not be empty");

            userForRegistor.Username = userForRegistor.Username.ToLower();
            if( await _repo.UserExists(userForRegistor.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = userForRegistor.Username
            };

            var CreatedUser = await _repo.Register(userToCreate,userForRegistor.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto) 
        {
            var userfromrepo = await _repo.Login(userForLoginDto.Username.ToLower(),userForLoginDto.Password);

            if(userfromrepo == null)
                return Unauthorized();

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userfromrepo.Id.ToString()),
                    new Claim(ClaimTypes.Name,userfromrepo.Username)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(_config.GetSection("AppSettings:token").Value));

                    var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

                    var tokenDescriptor = new SecurityTokenDescriptor 
                    {
                        Subject = new ClaimsIdentity(claims),
                        Expires = DateTime.Now.AddDays(1),
                        SigningCredentials = creds
                    };

                    var tokenHandler = new JwtSecurityTokenHandler();

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    return Ok(new {
                        token = tokenHandler.WriteToken(token)
                    });
        }  

        
    }
}