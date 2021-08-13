using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;

namespace API.Controllers
{
  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    //private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository/*, IMapper mapper*/)
    {
      _userRepository = userRepository;
      // _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers() {
        var users = await _userRepository.GetMembersAsync();     
        //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
        return Ok(users); 
    }

    [HttpGet("{username}")]
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
  }
}