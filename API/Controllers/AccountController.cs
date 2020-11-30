using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
      ITokenService tokenService, IMapper mapper)
    {
      _mapper = mapper;
      _userManager = userManager;
      _tokenService = tokenService;    
      _signInManager =  signInManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
            return BadRequest("Username is taken"); //creates a BadRequestObjectResult

        //using var hmac = new HMACSHA512(); //will dispose when var goes our os scope

        var user = _mapper.Map<AppUser>(registerDto);
        //var user = new AppUser()
      
        user.UserName = user.UserName.ToLower();
        //Replaced by .net identity
        //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        //user.PasswordSalt = hmac.Key; //it is randomly generated when hmac calss is instantiated

        // _context.Users.Add(user);
        // await _context.SaveChangesAsync();
        var result = await _userManager.CreateAsync(user, registerDto.Password); //it will take care of hashing the pwd

        if(!result.Succeeded)
            return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user,"Member");    
        
        if(!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);                

        return Ok(new UserDto()
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = await _tokenService.CreateToken(user) //generate jwt token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      var user = await _userManager.Users //Users is an IQueryable<User> object, so we can use the same functionality as with DbContext
          .Include(u => u.Photos)
          .SingleOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());
      if (user == null)
        return Unauthorized("Invalid username");

      //check password
      //using var hmac = new HMACSHA512(user.PasswordSalt);
      //var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      // if (!user.PasswordHash.SequenceEqual(computedHash))
      //   return Unauthorized("Invalid password");
      var result = await  _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,false);
      if(!result.Succeeded)
        return Unauthorized();

      return Ok(new UserDto()
      {
        Username = user.UserName,
        KnownAs = user.KnownAs,
        Gender = user.Gender,
        PhotoUrl = user.Photos?.FirstOrDefault(photo => photo.IsMain == true)?.Url,
        Token = await _tokenService.CreateToken(user) //generate jwt token
      });
    }

    private async Task<bool> UserExists(string username)
    {
      //var user = await Task.Factory.StartNew(()=>_context.Users.SingleOrDefault(u => u.UserName == username));
      //return (user != null);
      return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
    }
  }
}