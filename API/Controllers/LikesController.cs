using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class LikesController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;
    public LikesController(IUserRepository userRepository, ILikesRepository likesRrepository, IMapper mapper)
    {
      _likesRepository = likesRrepository;
      _userRepository = userRepository;
      _mapper = mapper;
    }

    //POST api/likes/nola
    [HttpPost("{username}")] //the user to be liked by the current user
    public async Task<ActionResult> AddLike(string username)
    {        
        var likedUser = await _userRepository.GetUserByUserNameAsync(username);
        
        if(likedUser==null) return NotFound();

        var sourceUserId = User.GetUserId();
        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

        if(sourceUser.UserName == username) return BadRequest("You cannot like yourself");

        var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
        
        if(userLike != null) return BadRequest("You already like this user");

        userLike = new UserLike(){
            SourceUserId = sourceUserId,
            LikedUserId = likedUser.Id
        };

        sourceUser.LikedUsers.Add(userLike);

        //_userRepository.Update(sourceUser);

        if(await _userRepository.SaveAllAsync())
            return Ok();

        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await _likesRepository.GetUsersLikes(likesParams);

        Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
        
        return Ok(users);
    }

  }
}