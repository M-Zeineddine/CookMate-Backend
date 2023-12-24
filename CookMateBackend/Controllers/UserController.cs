using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookMateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        /*[Authorize]*/
        [Route("getUserById")]
        public async Task<ResponseResult<UserDetailsModel>> GetUserDetails(int userId)
        {
            try
            {
                return await _userRepository.GetUserById(userId);

                /*if (userDetailsResult.IsSuccess == true)
                {
                    return Ok(userDetailsResult);
                }
                else
                {
                    return NotFound(userDetailsResult);
                }*/
            }
            catch (Exception ex)
            {
                return  new ResponseResult<UserDetailsModel>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Result = null
                };
            }
        }


        

        /*[HttpGet("{userId}/media")]
        public async Task<ActionResult<ResponseResult<List<UserMediasModel>>>> GetUserMedia(int userId)
        {
            try
            {
                var media = await _userRepository.GetUserMedia(userId);

                if (media.IsSuccess == true)
                {
                    return Ok(media);
                }
                else
                {
                    return NotFound(media);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseResult<List<UserMediasModel>>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Result = null
                });
            }
        }*/

    }
}
