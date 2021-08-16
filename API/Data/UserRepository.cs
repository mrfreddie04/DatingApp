using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Linq;
using AutoMapper;
using API.Entities;
using API.Interfaces;
using API.DTOs;
using API.Helpers;
using System;

namespace API.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
      return await _context.Users
        .Where( user => user.UserName == username)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
      var query = _context.Users.AsQueryable();

      query = query.Where(user => user.UserName != userParams.CurrentUserName);  
      query = query.Where(user => user.Gender == userParams.Gender);  

      var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
      var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

      query = query.Where(user => user.DateOfBirth >= minDob && user.DateOfBirth <= maxDob); 

      query = userParams.OrderBy switch {
          "created" => query.OrderByDescending( user => user.Created),
          _ => query.OrderByDescending( user => user.LastActive)
      };
        
      var pagedList = await PagedList<MemberDto>.CreateAsync(
          query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), 
          userParams.PageNumber, 
          userParams.PageSize
      );

      return pagedList;
      // return await _context.Users
      //   .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
      //   //.Include(u => u.Photos)
      //   .ToListAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUserNameAsync(string username)
    {
      return await _context.Users
        .Include(u => u.Photos)
        .SingleOrDefaultAsync(user => user.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      return await _context.Users
        .Include(u => u.Photos)
        .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    { 
      if( _context.ChangeTracker.HasChanges())  {
        var updates = await _context.SaveChangesAsync();
        return (updates > 0);
      }
      return false;
    }

    public void Update(AppUser user)
    {
      _context.Entry(user).State = EntityState.Modified;
    }
  }
}