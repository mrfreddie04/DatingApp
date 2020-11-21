using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
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

    // //GET api/users/1
    // [HttpGet("{id}")]
    // public async Task<ActionResult<AppUser>> GetUser(int id) {
    //     return await _repository.GetUserByIdAsync(id);
    //     //return await Task.Factory.StartNew(()=>_context.Users.Find(id));
    // }    
  }
}