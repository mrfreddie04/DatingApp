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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
      _unitOfWork = unitOfWork;
      _mapper = mapper;
      _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams) {
        
        var gender = await _unitOfWork.UserRepository.GetUserGenderAsync(User.GetUserName());
        userParams.CurrentUserName = User.GetUserName();

        if(string.IsNullOrEmpty(userParams.Gender))
          userParams.Gender = gender == "male" ? "female" : "male";

        // get PagedList<MemberDto>
        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);   

        // retrieve paging inro from PagedList and use it to add Pagination Header
        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        return Ok(users); 

        //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
    }

    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDto>> GetUser(string username) {
        // var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
        // var userToReturn = _mapper.Map<MemberDto>(user);
        var user = await _unitOfWork.UserRepository.GetMemberAsync(username);
        return user;
        // if(user != null) {
        //   var userToReturn = _mapper.Map<MemberDto>(user);
        //   return Ok(userToReturn); 
        // }
        // //   return Ok(user); 
        // return NotFound();
        //return await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
    }    

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) {
        //get username from the token (User claim principal is available in the parent class - ControllerBase)
        //get user entity from the DB
        var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

        //use mapper to update the user
        _mapper.Map(memberUpdateDto, user);

        //add tracking info to the db context - change status of the user object to EntityState.Modified
        _unitOfWork.UserRepository.Update(user);

        //save changes to the db
        if(await _unitOfWork.CompleteAsync())
          return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) {
      //get user - we are eagerly loading photos
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

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
      _unitOfWork.UserRepository.Update(user);

        //save changes to the db
      if(await _unitOfWork.CompleteAsync()) {
        return CreatedAtRoute("GetUser",new {username=user.UserName},_mapper.Map<PhotoDto>(photo)); 
        //return _mapper.Map<PhotoDto>(photo);    
      }
          
      return BadRequest("Problem add photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId) {
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

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
      _unitOfWork.UserRepository.Update(user);

        //save changes to the db
      if(await _unitOfWork.CompleteAsync()) {
        return NoContent();
      }      

     return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId) {
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

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

      _unitOfWork.UserRepository.Update(user);

        //save changes to the db
      if(await _unitOfWork.CompleteAsync()) {
        return Ok();
      }       

      return BadRequest("Failed to delete photo");
    }
  }
}  