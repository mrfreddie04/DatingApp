using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
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

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = _context.Users.AsQueryable();

        if(!string.IsNullOrEmpty(userParams.CurrentUserName))
          query = query.Where(u=>u.UserName != userParams.CurrentUserName); 

        if(!string.IsNullOrEmpty(userParams.Gender))
          query = query.Where(u=>u.Gender == userParams.Gender); 

        var minDob = DateTime.Today.AddYears(-userParams.MaxAge-1);
        query = query.Where(u=>u.DateOfBirth >= minDob);               

        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
        query = query.Where(u=>u.DateOfBirth <= maxDob);    

        // if(userParams.OrderBy=="lastActive") 
        //   query = query.OrderByDescending(u=>u.LastActive);

        // if(userParams.OrderBy=="lastActive") 
        //   query = query.OrderByDescending(u=>u.Created);

        // new switch expression c#8
        query = userParams.OrderBy switch 
        {
          "created" => query.OrderByDescending(u=>u.Created),
          "lastActive" => query.OrderByDescending(u=>u.LastActive),
          _ => query.OrderBy(u=>u.UserName)
        }; 
          
        var queryDto = query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider) //projects the query results to the mapped object
          .AsNoTracking(); //disable EF tracking, as it is a reand-only operation (no db updates)

        return await PagedList<MemberDto>.CreateAsync(queryDto,userParams.PageNumber,userParams.PageSize);
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