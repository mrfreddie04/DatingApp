using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLikeAsync(int sourseUserId, int likedUserId);
        Task<AppUser> GetUserWithLikesAsync(int userId);
        Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams);
    }
}