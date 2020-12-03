using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Extensions;
using System.Linq;
using System;
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

    //GET api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams )
    {
      var username = User.GetUsername();
      var gender = await _unitOfWork.UserRepository.GetUserGenderAsync(username);
      userParams.CurrentUserName = username;
      
      if(String.IsNullOrEmpty(userParams.Gender))
        userParams.Gender = gender == "male" ? "female" : "male";

      var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

      //Controller base class gives us access to the response object
      Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);

      return Ok(users);      
    }

    //GET api/users/nola, assign a name to the route
    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
      var currentUsername = User.GetUsername();
      bool isCurrentUser = (username == currentUsername); 
      var userDto = await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser);

      return Ok(userDto);
      //return await Task.Factory.StartNew(()=>_context.Users.Find(id));
    }

    //POST api/users
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
      //get the user name from the token! 
      //User is a property of ControllerBase class
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());

      _mapper.Map(memberUpdateDto,user);

      _unitOfWork.UserRepository.Update(user);

      var result = await _unitOfWork.Complete();

      if(result)
        return NoContent();

      return BadRequest("Failed to update user");  
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());

      var result = await _photoService.AddPhotoAsync(file);
      
      if(result.Error != null) return BadRequest(result.Error.Message);

      var photo = new Photo() 
      {
        Url = result.SecureUrl.AbsoluteUri,
        PublicId = result.PublicId
      };

      if(user.Photos.Count==0)
      {
        //newly added first photo are not automatically tagged main
        photo.IsMain = false;
      }

      user.Photos.Add(photo);

      if(await _unitOfWork.Complete())     
      { 
        //that would not work, because "GetUser" route requires a parameter  
        //return CreatedAtRoute("GetUser",_mapper.Map<PhotoDto>(photo));  
        //it populates Location header in the http response with: https://localhost:5001/api/Users/{{username}}
        return CreatedAtRoute("GetUser",new {username=user.UserName},_mapper.Map<PhotoDto>(photo));  
      } 
      
      return BadRequest("Failed to add photo");  
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());

      var photo = user.Photos.FirstOrDefault(photo=>photo.Id==photoId);
      if(photo == null)
        return BadRequest("Failed to update photo");
      if(photo.IsMain)
        return BadRequest("This is already your main photo");

      var currentMain = user.Photos.FirstOrDefault(photo=>photo.IsMain==true);
      if(currentMain != null)
        currentMain.IsMain = false;

      photo.IsMain = true;

      //_repository.Update(user);

      if(await _unitOfWork.Complete())
        return NoContent();   

      return BadRequest("Failed to set main photo");      
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
      var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());

      var photo = user.Photos.FirstOrDefault(photo=>photo.Id==photoId);

      if(photo == null) return NotFound();
      
      if(photo.IsMain) return BadRequest("You cannot delete your main photo"); 

      if(!String.IsNullOrWhiteSpace(photo.PublicId))
      {
        var result = await _photoService.DeletePhotoAsync(photo.PublicId);    

        if(result.Error != null) return BadRequest(result.Error.Message);   
      }

      user.Photos.Remove(photo);

      if(await _unitOfWork.Complete())     
      { 
        //that would not work, because "GetUser" route requires a parameter  
        //return CreatedAtRoute("GetUser",_mapper.Map<PhotoDto>(photo));  
        //it populates Location header in the http response with: https://localhost:5001/api/Users/{{username}}
        return Ok();  
      } 
      
      return BadRequest("Failed to delete the photo");  
    }

    // //GET api/users/1
    // [HttpGet("{id}")]
    // public async Task<ActionResult<AppUser>> GetUser(int id) {
    //     return await _repository.GetUserByIdAsync(id);
    //     //return await Task.Factory.StartNew(()=>_context.Users.Find(id));
    // }    
  }
}