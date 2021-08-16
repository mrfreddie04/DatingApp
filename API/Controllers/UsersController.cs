using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using API.Interfaces;
using API.Extensions;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
      _userRepository = userRepository;
      _mapper = mapper;
      _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams) {
        
        var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
        userParams.CurrentUserName = user.UserName;

        if(string.IsNullOrEmpty(userParams.Gender))
          userParams.Gender = user.Gender == "male" ? "female" : "male";

        // get PagedList<MemberDto>
        var users = await _userRepository.GetMembersAsync(userParams);   

        // retrieve paging inro from PagedList and use it to add Pagination Header
        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        return Ok(users); 

        //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
    }

    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDto>> GetUser(string username) {
        // var user = await _userRepository.GetUserByUserNameAsync(username);
        // var userToReturn = _mapper.Map<MemberDto>(user);
        var user = await _userRepository.GetMemberAsync(username);
        return user;
        // if(user != null) {
        //   var userToReturn = _mapper.Map<MemberDto>(user);
        //   return Ok(userToReturn); 
        // }
        // //   return Ok(user); 
        // return NotFound();
        //return await _userRepository.GetUserByUserNameAsync(username);
    }    

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) {
        //get username from the token (User claim principal is available in the parent class - ControllerBase)
        //get user entity from the DB
        var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

        //use mapper to update the user
        _mapper.Map(memberUpdateDto, user);

        //add tracking info to the db context - change status of the user object to EntityState.Modified
        _userRepository.Update(user);

        //save changes to the db
        if(await _userRepository.SaveAllAsync())
          return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) {
      //get user - we are eagerly loading photos
      var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

      //upload to cloudinary
      var results = await _photoService.AddPhotoAsync(file);

      if(results.Error != null)
        return BadRequest(results.Error.Message);

      //create Photo
      var photo = new Photo() {
        Url = results.SecureUrl.AbsoluteUri,
        PublicId = results.PublicId,        
        IsMain = false
      };

      if(user.Photos.Count == 0) {
        photo.IsMain = true;
      }

      //add Photo to the Photos collection
      user.Photos.Add(photo);

      //update user record
      _userRepository.Update(user);

        //save changes to the db
      if(await _userRepository.SaveAllAsync()) {
        return CreatedAtRoute("GetUser",new {username=user.UserName},_mapper.Map<PhotoDto>(photo)); 
        //return _mapper.Map<PhotoDto>(photo);    
      }
          
      return BadRequest("Problem add photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId) {
      var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

      var photo = user.Photos.FirstOrDefault((photo)=>photo.Id == photoId);
      if(photo == null) 
        return BadRequest("Failed to update photo");
      if(photo.IsMain) 
        return BadRequest("This is already your main photo");        

      var currentMainPhoto = user.Photos.FirstOrDefault((photo)=>photo.IsMain);
      if(currentMainPhoto != null) 
        currentMainPhoto.IsMain = false;

      photo.IsMain = true;  

      //update user record
      _userRepository.Update(user);

        //save changes to the db
      if(await _userRepository.SaveAllAsync()) {
        return NoContent();
      }      

     return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId) {
      var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

      var photo = user.Photos.FirstOrDefault((photo)=>photo.Id == photoId);

      if(photo == null) 
        return BadRequest("Failed to delete photo");
      
      if(photo.IsMain) 
        return BadRequest("You cannot delete your main photo");
      
      if(photo.PublicId != null) {
        var results = await _photoService.DeletePhotoAsync(photo.PublicId);        
        if(results.Error != null)
          return BadRequest(results.Error.Message);        
      }        

      user.Photos.Remove(photo);

      _userRepository.Update(user);

        //save changes to the db
      if(await _userRepository.SaveAllAsync()) {
        return Ok();
      }       

      return BadRequest("Failed to delete photo");
    }
  }
}  