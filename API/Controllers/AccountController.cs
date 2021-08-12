using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.DTOs;
using API.Interfaces;

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
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) {
        //get random salt
        //calculate salted pwd hash
        //add user to the db
        //save
        //return user
        // var users = await _context.Users.ToListAsync();     
        // return users[0]; 

        if(await UserExists(registerDto.Username)) {
          return BadRequest("Username is taken"); //ActionResult lets us return status codes and data 
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser(){
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key         
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        return new UserDto(){
          Username = user.UserName,
          Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) {
        var user = await _context.Users.SingleOrDefaultAsync(
          (AppUser user) => user.UserName.ToLower() == loginDto.Username.ToLower());

        if( user == null)
          return Unauthorized("Invalid username"); 
        
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if(user.PasswordHash.Length != passwordHash.Length)
          return Unauthorized("Invalid password"); 

        for(var i = 0; i < user.PasswordHash.Length; i++) {
            if(passwordHash[i] != user.PasswordHash[i])
              return Unauthorized("Invalid password"); 
        }
        
        return new UserDto(){
          Username = user.UserName,
          Token = _tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists(string username) {
        return await _context.Users.AnyAsync( (AppUser user) => user.UserName.ToLower() == username.ToLower());
    }
  }
}