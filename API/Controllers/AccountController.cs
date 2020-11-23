using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            if(await UserExists(registerDto.Username))
                return BadRequest("Username is taken"); //creates a BadRequestObjectResult
            
            using var hmac= new HMACSHA512(); //will dispose when var goes our os scope

            var user = new AppUser(){
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key //it is randomly generated when hmac calss is instantiated
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new UserDto(){
                Username = user.UserName,
                Token = _tokenService.CreateToken(user) //generate jwt token
            });           
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u=>u.UserName == loginDto.Username.ToLower());
            if(user==null)
                return Unauthorized("Invalid username");
            
            //check password
            using var hmac= new HMACSHA512(user.PasswordSalt); 
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            if(!user.PasswordHash.SequenceEqual(computedHash))
                return Unauthorized("Invalid password");
            
            return Ok(new UserDto(){
                Username = user.UserName,
                PhotoUrl = user.Photos?.FirstOrDefault(photo=>photo.IsMain==true)?.Url,
                Token = _tokenService.CreateToken(user) //generate jwt token
            });
        }

        private async Task<bool> UserExists(string username)
        {
            //var user = await Task.Factory.StartNew(()=>_context.Users.SingleOrDefault(u => u.UserName == username));
            //return (user != null);
            return await _context.Users.AnyAsync(u => u.UserName == username.ToLower());
        }
    }
}