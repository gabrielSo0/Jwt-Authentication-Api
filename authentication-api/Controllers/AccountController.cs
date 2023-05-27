using authentication_api.Data;
using authentication_api.DTOs;
using authentication_api.Interfaces;
using authentication_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace authentication_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context,
                                ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterDTO registerDTO)
        {

            if (await UserExists(registerDTO.Username)) return BadRequest("Username if taken");

            // Creating an instance of the cryptograph algorithm HMACSHA512
            // If you need more information, access: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha512?view=net-7.0
            using var hmac = new HMACSHA512();

            // Creating a instance of the User entity, transforming the password into a hash using a salt key.
            // For more information about password salt: https://auth0.com/blog/adding-salt-to-hashing-a-better-way-to-store-passwords/
            var user = new User
            {
                UserName = registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginDTO.Username);

            if (user == null) return Unauthorized();

            // Initializing a instance of HMACSHA512 passing the user salt key
            using var hmac = new HMACSHA512(user.PasswordSalt);

            // using the salt key and the hmac instance, I can create a password hash from the password that the user passed.
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            // Check if the password hash from the database is equal to the password the user inserted.
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
