using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
  public class TokenService : ITokenService
  {
    private readonly SymmetricSecurityKey _key;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(IConfiguration config, UserManager<AppUser> userManager)
    {
      _userManager = userManager;
      _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    public async Task<string> CreateToken(AppUser user)
    {
      //add claims
      var claims = new List<Claim>()
      {
          new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
          new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
      };

      var roles = await _userManager.GetRolesAsync(user);  
      claims.AddRange(roles.Select(role=>new Claim(ClaimTypes.Role,role)));

      //create credentials
      var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

      //describe how token would look
      var tokenDescriptor = new SecurityTokenDescriptor()
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(7),
        SigningCredentials = creds
      };

      //create and write the token
      var tokenHandler = new JwtSecurityTokenHandler();

      var token = tokenHandler.CreateToken(tokenDescriptor);

      return tokenHandler.WriteToken(token);
    }
  }
}