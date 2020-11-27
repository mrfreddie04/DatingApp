using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class LikesRepository : ILikesRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public LikesRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
    {
      //int[] pk = new int[]{sourceUserId,likedUserId};
      return await _context.Likes.FindAsync(sourceUserId, likedUserId);
      //return await _context.Likes.SingleOrDefaultAsync(l=>l.SourceUserId==sourceUserId && l.LikedUserId==likedUserId);
    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
      return await _context.Users
          .Include(u => u.LikedUsers)
          .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<PagedList<LikeDto>> GetUsersLikes(LikesParams likesParams)
    {
      var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
      var likes = _context.Likes.AsQueryable();

      if (likesParams.Predicate == "liked") //the users that userId has liked    
      {
        //likes = likes.Where(l => l.SourceUserId == userId);
        users = likes.Where(l => l.SourceUserId == likesParams.UserId).Select(l => l.LikedUser);
      }

      if (likesParams.Predicate == "likedBy") //the users that that liked userId
      {
        //likes = likes.Where(l => l.LikedUserId == userId);
        users = likes.Where(l => l.LikedUserId == likesParams.UserId).Select(l => l.SourceUser);
      }

      //return await users.ProjectTo<LikeDto>(_mapper.ConfigurationProvider).ToListAsync();
      var queryDto = users.Select(u=>new LikeDto(){
            Id = u.Id,
            Username = u.UserName,
            Age = u.DateOfBirth.CalculateAge(),
            KnownAs = u.KnownAs,
            PhotoUrl = u.Photos.FirstOrDefault(p=>p.IsMain==true).Url,
            City = u.City
      }).AsNoTracking();

      return await PagedList<LikeDto>.CreateAsync(queryDto, likesParams.PageNumber, likesParams.PageSize);      
    }

  }
}