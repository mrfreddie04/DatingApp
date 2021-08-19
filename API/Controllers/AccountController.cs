using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.DTOs;
using API.Interfaces;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(
      UserManager<AppUser> userManager, 
      SignInManager<AppUser> signInManager,
      ITokenService tokenService,
      IMapper mapper
    )
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _tokenService = tokenService;
      _mapper = mapper;
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

      var user = _mapper.Map<AppUser>(registerDto);

      // to be address by Core Identity
      // using var hmac = new HMACSHA512();      

      // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
      // user.PasswordSalt = hmac.Key;         

      var result = await _userManager.CreateAsync(user, registerDto.Password);
      if(!result.Succeeded) {
        return BadRequest(result.Errors); 
      }
    
      var roleResult = await _userManager.AddToRoleAsync(user, "Member");
      if(!result.Succeeded) {
        return BadRequest(result.Errors); 
      }      

      return new UserDto(){
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) {
  
      var user = await _userManager.Users
        .Include( user => user.Photos)
        .SingleOrDefaultAsync(
          (user) => user.UserName.ToLower() == loginDto.Username.ToLower());

      // var user = await _userRepository.GetUserByUserNameAsync(loginDto.Username.ToLower());

      if( user == null)
        return Unauthorized("Invalid username"); 

      var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
      if(!result.Succeeded) {
        return Unauthorized(); 
      }      
      
      // to be address by Core Identity
      // using var hmac = new HMACSHA512(user.PasswordSalt);
      // var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      // if(user.PasswordHash.Length != passwordHash.Length)
      //   return Unauthorized("Invalid password"); 

      // for(var i = 0; i < user.PasswordHash.Length; i++) {
      //     if(passwordHash[i] != user.PasswordHash[i])
      //       return Unauthorized("Invalid password"); 
      // }
      
      return new UserDto(){
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,
        Gender = user.Gender,
        PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain)?.Url
      };
    }

    private async Task<bool> UserExists(string username) {
      //var user = await _userManager.FindByNameAsync(username.ToLower());
      //return (user != null);
      return await _userManager.Users.AnyAsync( (AppUser user) => user.UserName.ToLower() == username.ToLower());
    }
  }
}