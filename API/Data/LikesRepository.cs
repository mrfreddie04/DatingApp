using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.Extensions;
using API.Helpers;

namespace API.Data
{
  public class LikesRepository : ILikesRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public LikesRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<UserLike> GetUserLikeAsync(int sourseUserId, int likedUserId)
    {
      return await _context.Likes.FindAsync(sourseUserId, likedUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams)
    {
        // var query = predicate switch {
	    //     "liked" => _context.Likes.Where( ul => ul.SourceUserId == userId).Select(ul => new {userId = ul.LikedUserId}),
	    //     "likedBy" => _context.Likes.Where( ul => ul.LikedUserId == userId).Select(ul => new {userId = ul.SourceUserId}),
        //     _ => _context.Likes.Where( ul => ul.SourceUserId == userId).Select(ul => new {userId = ul.LikedUserId})
        // };    

        // var userlikes = query.Join( 
        //     _context.Users, 
        //     like => like.userId, 
        //     user => user.Id,
        //     (like, user) => user
        // ).ProjectTo<LikeDto>(_mapper.ConfigurationProvider).AsNoTracking();

        // return await userlikes.ToListAsync();

        var users = _context.Users.OrderBy(user => user.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if( likesParams.Predicate == "liked") {
            likes = likes.Where( ul => ul.SourceUserId == likesParams.UserId);
            users = likes.Select( ul => ul.LikedUser);
        } 
        if( likesParams.Predicate == "likedBy") {
            likes = likes.Where( ul => ul.LikedUserId == likesParams.UserId);
            users = likes.Select( ul => ul.SourceUser);
        }      

        var likedUsers = users.Select( user => new LikeDto(){
            Id = user.Id,
            Username = user.UserName,
            Age = user.DateOfBirth.CalculateAge(),
            KnownAs = user.KnownAs,
            PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain).Url,
            City = user.City
        }).AsNoTracking();

        //return await users.ProjectTo<LikeDto>(_mapper.ConfigurationProvider).AsNoTracking().ToListAsync();
        return await PagedList<LikeDto>.CreateAsync(
            likedUsers, 
            likesParams.PageNumber,
            likesParams.PageSize
        );
    }

    public async Task<AppUser> GetUserWithLikesAsync(int userId)
    {
      return await _context.Users
        .Include( au => au.LikedUsers)
        .FirstOrDefaultAsync( au => au.Id == userId);
    }
  }
}