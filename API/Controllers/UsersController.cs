using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository repository, IMapper mapper)
    {
      _mapper = mapper;
      _repository = repository;
    }

    //GET api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
      var usersDto = await _repository.GetMembersAsync();

      return Ok(usersDto);
      //var users = await Task.Factory.StartNew(_context.Users.OrderBy(u=>u.Id).ToList<AppUser>);
    }


    //GET api/users/nola
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
      var userDto = await _repository.GetMemberAsync(username);

      return userDto;
      //return await Task.Factory.StartNew(()=>_context.Users.Find(id));
    }

    //POST api/users
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
      //get the user name from the token! 
      //User is a property of COntrollerBase class
      var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var user = await _repository.GetUserByUserNameAsync(username);

      _mapper.Map(memberUpdateDto,user);

      _repository.Update(user);

      var result = await _repository.SaveAllAsync();

      if(result)
        return NoContent();

      return BadRequest("Failed to update user");  
    }

    // //GET api/users/1
    // [HttpGet("{id}")]
    // public async Task<ActionResult<AppUser>> GetUser(int id) {
    //     return await _repository.GetUserByIdAsync(id);
    //     //return await Task.Factory.StartNew(()=>_context.Users.Find(id));
    // }    
  }
}