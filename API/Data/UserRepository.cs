using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
      //throw new System.NotImplementedException();
      return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUserNameAsync(string username)
    {
      //return await Task.Factory.StartNew(()=>_context.Users.Single(u=>u.UserName==username));
      return await _context.Users
          .Include(u => u.Photos)
          .SingleOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
        //ConfigurationProvider - configurations defined in AutoMapperProfiles
      return await _context.Users
          .Where(u => u.UserName == username)
          //.Include(u => u.Photos)
          .ProjectTo<MemberDto>(_mapper.ConfigurationProvider) //projects the qury results to the mapped object
          .SingleOrDefaultAsync();

      // return await _context.Users
      //     .Where(u=>u.UserName==username)
      //     .Include(u=>u.Photos);


    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      return await _context.Users.Include(u => u.Photos).OrderBy(u => u.UserName).ToListAsync();
    }

    public async Task<IEnumerable<MemberDto>> GetMembersAsync()
    {
      return await _context.Users
          //.Include(u => u.Photos)
          .ProjectTo<MemberDto>(_mapper.ConfigurationProvider) //projects the qury results to the mapped object
          .ToListAsync();
    }


    public async Task<bool> SaveAllAsync()
    {
      // if(_context.ChangeTracker.HasChanges())
      // {
      //     await _context.SaveChangesAsync();
      //     return true;
      // }
      // return false;

      return (await _context.SaveChangesAsync()) > 0;
    }

    public void Update(AppUser user)
    {
      //mark entity as modified
      _context.Entry<AppUser>(user).State = EntityState.Modified;
    }

  }
}