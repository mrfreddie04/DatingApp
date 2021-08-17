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
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
      _userRepository = userRepository;
      _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username) 
    {
        var sourceUserId = User.GetUserId();
        var sourceUser = await _likesRepository.GetUserWithLikesAsync(sourceUserId);
        var likedUser = await _userRepository.GetUserByUserNameAsync(username);

        if(likedUser == null)
            return NotFound("User not found");

        if(sourceUser.UserName == likedUser.UserName)    
            return BadRequest("You cannot like yourself"); 

        var userLike = await _likesRepository.GetUserLikeAsync(sourceUserId, likedUser.Id);    
        if(userLike != null)
            return BadRequest("You already like this user");  

        userLike = new UserLike(){
            SourceUserId = sourceUserId,
            LikedUserId = likedUser.Id
        };

        sourceUser.LikedUsers.Add(userLike);

        _userRepository.Update(sourceUser);

        if( await _userRepository.SaveAllAsync())
            return Ok();

        return BadRequest("Failed to like user");
    }    

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams) {
        likesParams.UserId = User.GetUserId();

        var users = await _likesRepository.GetUserLikesAsync(likesParams);

        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        return Ok(users);
    }
  }
}