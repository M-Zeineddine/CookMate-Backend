using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookMateBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly CookMateContext _context; // Your DbContext or service that contains the method
        private readonly IFollowRepository _followRepository;


        public FollowController(CookMateContext context, IFollowRepository followRepository)
        {
            _context = context;
            _followRepository = followRepository;
        }

        [HttpGet("my-followers")]
        public async Task<ResponseResult<List<FollowerInfo>>> GetMyFollowers(int loggedInUserId, [FromQuery] string search)
        {
            return await _followRepository.GetMyFollowersWithStatusAsync(loggedInUserId, search);
        }


        [HttpGet("my-followings")]
        public async Task<ResponseResult<List<FollowerInfo>>> GetMyFollowings(int loggedInUserId, [FromQuery] string search)
        {
            return await _followRepository.GetMyFollowingsWithStatusAsync(loggedInUserId, search);
        }

        [HttpPost("follow")]
        public async Task<ActionResult<ResponseResult<bool>>> FollowUser([FromBody] UserFollowModel model)
        {
            return await _followRepository.FollowUserAsync(model.LoggedInUserId, model.TargetUserId);
        }

        [HttpPost("unfollow")]
        public async Task<ActionResult<ResponseResult<bool>>> UnfollowUser([FromBody] UserFollowModel model)
        {
            return await _followRepository.UnfollowUserAsync(model.LoggedInUserId, model.TargetUserId);

        }

    }

}
