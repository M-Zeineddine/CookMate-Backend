


using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Models.InputModels;

namespace CookMateBackend.Data.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        public readonly CookMateContext _CookMateContext;

        public FollowRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }

        public async Task<ResponseResult<List<FollowerInfo>>> GetMyFollowersWithStatusAsync(int loggedInUserId, string search = null)
        {
            try
            {
                var followersQuery = _CookMateContext.Followers
                    .Where(f => f.UserId == loggedInUserId)
                    .Select(f => new { f.FollowerNavigation, IsFollowingBack = _CookMateContext.Followers.Any(fb => fb.UserId == f.FollowerId && fb.FollowerId == loggedInUserId) });

                if (!string.IsNullOrEmpty(search))
                {
                    followersQuery = followersQuery.Where(f => f.FollowerNavigation.Username.Contains(search));
                }

                var followers = await followersQuery
                    .Select(f => new FollowerInfo
                    {
                        FollowerId = f.FollowerNavigation.Id,
                        FollowerName = f.FollowerNavigation.Username,
                        IsFollowingBack = f.IsFollowingBack
                    })
                    .ToListAsync();

                return new ResponseResult<List<FollowerInfo>>
                {
                    IsSuccess = true,
                    Message = "Followers retrieved successfully.",
                    Result = followers
                };
            }
            catch (Exception ex)
            {
                // Handle the exception as needed (e.g., logging)
                return new ResponseResult<List<FollowerInfo>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving followers: {ex.Message}",
                    Result = null
                };
            }
        }



        public async Task<ResponseResult<List<FollowerInfo>>> GetMyFollowingsWithStatusAsync(int loggedInUserId, string search = null)
        {
            try
            {
                var followingsQuery = _CookMateContext.Followers
                    .Where(f => f.FollowerId == loggedInUserId)
                    .Select(f => new
                    {
                        FollowingId = f.UserId,
                        FollowingUsername = f.User.Username,
                        IsFollowingBack = _CookMateContext.Followers.Any(fb => fb.UserId == loggedInUserId && fb.FollowerId == f.UserId)
                    });

                if (!string.IsNullOrEmpty(search))
                {
                    followingsQuery = followingsQuery.Where(f => f.FollowingUsername.Contains(search));
                }

                var followings = await followingsQuery
                    .Select(f => new FollowerInfo
                    {
                        FollowerId = f.FollowingId,
                        FollowerName = f.FollowingUsername,
                        IsFollowingBack = f.IsFollowingBack
                    })
                    .ToListAsync();

                return new ResponseResult<List<FollowerInfo>>
                {
                    IsSuccess = true,
                    Message = "Followings retrieved successfully.",
                    Result = followings
                };
            }
            catch (Exception ex)
            {
                // Handle the exception as needed
                return new ResponseResult<List<FollowerInfo>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving followings: {ex.Message}",
                    Result = null
                };
            }
        }






        public async Task<ResponseResult<bool>> FollowUserAsync(int loggedInUserId, int targetUserId)
        {
            var result = new ResponseResult<bool>();

            if (loggedInUserId == targetUserId)
            {
                result.IsSuccess = false;
                result.Message = "You cannot follow yourself.";
                return result;
            }

            var existingFollow = await _CookMateContext.Followers
                .FirstOrDefaultAsync(f => f.UserId == targetUserId && f.FollowerId == loggedInUserId);

            if (existingFollow != null)
            {
                result.IsSuccess = false;
                result.Message = "You are already following this user.";
                return result;
            }

            var follow = new Follower
            {
                UserId = targetUserId,
                FollowerId = loggedInUserId,
                CreatedAt = DateTime.UtcNow // Assuming you want to store the time in UTC
            };

            await _CookMateContext.Followers.AddAsync(follow);
            await _CookMateContext.SaveChangesAsync();

            result.IsSuccess = true;
            result.Message = "You are now following the user.";
            result.Result = true;

            return result;
        }


        public async Task<ResponseResult<bool>> UnfollowUserAsync(int loggedInUserId, int targetUserId)
        {
            var result = new ResponseResult<bool>();

            var followRecord = await _CookMateContext.Followers
                .FirstOrDefaultAsync(f => f.UserId == targetUserId && f.FollowerId == loggedInUserId);

            if (followRecord == null)
            {
                result.IsSuccess = false;
                result.Message = "You are not following this user.";
                return result;
            }

            _CookMateContext.Followers.Remove(followRecord);
            await _CookMateContext.SaveChangesAsync();

            result.IsSuccess = true;
            result.Message = "You have unfollowed the user.";
            result.Result = true;

            return result;
        }


    }
}
