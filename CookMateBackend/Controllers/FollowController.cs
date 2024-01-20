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

        [HttpGet("my-followers/{loggedInUserId}")]
        public async Task<ActionResult<List<FollowerInfo>>> GetMyFollowers(int loggedInUserId, [FromQuery] string search)
        {
            return await _followRepository.GetMyFollowersWithStatusAsync(loggedInUserId, search);
        }

        [HttpGet("my-followings/{loggedInUserId}")]
        public async Task<ActionResult<List<FollowerInfo>>> GetMyFollowings(int loggedInUserId, [FromQuery] string search)
        {
            return await _followRepository.GetMyFollowingsWithStatusAsync(loggedInUserId, search);
        }

        [HttpPost("follow")]
        public async Task<ActionResult<ResponseResult<bool>>> FollowUser(int loggedInUserId, int targetUserId)
        {
            return await _followRepository.FollowUserAsync(loggedInUserId, targetUserId);
        }


        [HttpPost("unfollow/{targetUserId}")]
        public async Task<ActionResult<ResponseResult<bool>>> UnfollowUser(int loggedInUserId, int targetUserId)
        {
            var response = await _followRepository.UnfollowUserAsync(loggedInUserId, targetUserId);
            return response.IsSuccess == true ? Ok(response) : BadRequest(response);
        }
    }

}
