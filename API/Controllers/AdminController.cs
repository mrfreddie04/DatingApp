using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using System;

namespace API.Controllers
{
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
    {
      _photoService = photoService;
      _unitOfWork = unitOfWork;
      _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsersWithRoles()
    {
      var users = await _userManager.Users
          .Include(u => u.UserRoles)
          .ThenInclude(ur => ur.Role)
          .OrderBy(u => u.UserName)
          .Select(u => new
          {
            Id = u.Id,
            Username = u.UserName,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
          })
          .ToListAsync();

      return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult<IEnumerable<string>>> EditUserRoles(string username, [FromQuery] string roles)
    {
      var selectedRoles = roles.Split(",").ToArray();

      var user = await _userManager.FindByNameAsync(username);
      if (user == null)
        return NotFound("Could nt find user");

      var userRoles = await _userManager.GetRolesAsync(user);

      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

      if (!result.Succeeded)
        return BadRequest("Failed to add to roles");

      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if (!result.Succeeded)
        return BadRequest("Failed to remove from roles");

      return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForApproval()
    {
      var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
      return Ok(photos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
      var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
      if (photo == null)
        return NotFound("Photo not found");
      if (photo.isApproved)
        return NotFound("Photo is already approved");

      photo.isApproved = true;

      var user = await _unitOfWork.UserRepository.GetUserByPhotoIdAsync(photoId);

      photo.IsMain = !user.Photos.Any(p => p.IsMain);

      var result = await _unitOfWork.Complete();

      if (result)
        return Ok();

      return BadRequest("Failed to approve photo");
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null)
            return NotFound("Photo not found");
        if (photo.isApproved)
            return BadRequest("Cannot reject. Photo is already approved");            

        if(!String.IsNullOrWhiteSpace(photo.PublicId))  
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);   
        }

        _unitOfWork.PhotoRepository.RemovePhoto(photo);

        if(await _unitOfWork.Complete())
            return Ok();

        return BadRequest("Failed to reject photo");
    }

  }
}