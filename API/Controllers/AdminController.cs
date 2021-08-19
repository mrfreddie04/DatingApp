using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Entities;

namespace API.Controllers
{
    public class AdminController: BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")] 
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsersWithRoles() 
        {
            var users = await _userManager.Users
                .Include(au => au.UserRoles)
                .ThenInclude(aur => aur.Role)
                .OrderBy( au => au.UserName)
                .Select( au => new 
                {  
                    au.Id,
                    Username = au.UserName, 
                    Roles = au.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration() 
        {
            return Ok("Admins or moderators can see this!");
        }        

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]         
        public async Task<ActionResult<IEnumerable<string>>> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",");
            
            var user = await _userManager.FindByNameAsync(username.ToLower());
            
            if(user == null) return BadRequest("Couldn't find user");

            var userRoles = await _userManager.GetRolesAsync(user);    

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            
            if(!result.Succeeded) return BadRequest(result.Errors); 

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded) return BadRequest(result.Errors); 

            return Ok(await _userManager.GetRolesAsync(user));
        }
    }
}