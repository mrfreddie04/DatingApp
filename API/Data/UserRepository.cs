using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Linq;
using AutoMapper;
using API.Entities;
using API.Interfaces;
using API.DTOs;

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

    public async Task<IEnumerable<MemberDto>> GetMembersAsync()
    {
      return await _context.Users
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        //.Include(u => u.Photos)
        .ToListAsync();
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