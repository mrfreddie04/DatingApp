using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Extensions;
using API.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
  [Authorize]
  public class LikesController : BaseApiController
  {
    private readonly IUnitOfWork _unitOfWork;

    public LikesController(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username) 
    {
        var sourceUserId = User.GetUserId();
        var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikesAsync(sourceUserId);
        var likedUser = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);

        if(likedUser == null)
            return NotFound("User not found");

        if(sourceUser.UserName == likedUser.UserName)    
            return BadRequest("You cannot like yourself"); 

        var userLike = await _unitOfWork.LikesRepository.GetUserLikeAsync(sourceUserId, likedUser.Id);    
        if(userLike != null)
            return BadRequest("You already like this user");  

        userLike = new UserLike(){
            SourceUserId = sourceUserId,
            LikedUserId = likedUser.Id
        };

        sourceUser.LikedUsers.Add(userLike);

        _unitOfWork.UserRepository.Update(sourceUser);

        if( await _unitOfWork.CompleteAsync())
            return Ok();

        return BadRequest("Failed to like user");
    }    

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams) {
        likesParams.UserId = User.GetUserId();

        var users = await _unitOfWork.LikesRepository.GetUserLikesAsync(likesParams);

        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        return Ok(users);
    }
  }
}