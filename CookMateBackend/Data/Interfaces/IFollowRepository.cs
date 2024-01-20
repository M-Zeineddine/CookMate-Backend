using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IFollowRepository
    {
        Task<List<FollowerInfo>> GetMyFollowersWithStatusAsync(int loggedInUserId, string search = null);
        Task<ResponseResult<bool>> FollowUserAsync(int loggedInUserId, int targetUserId);
        Task<ResponseResult<bool>> UnfollowUserAsync(int loggedInUserId, int targetUserId);
        Task<List<FollowerInfo>> GetMyFollowingsWithStatusAsync(int loggedInUserId, string search = null);

    }
}
