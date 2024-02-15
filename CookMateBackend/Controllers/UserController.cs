using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
using CookMateBackend.Models;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CookMateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        public readonly CookMateContext _CookMateContext;


        public UserController(IUserRepository userRepository, CookMateContext cookMateContext, IPostRepository postRepository)
        {
            _userRepository = userRepository;
            _CookMateContext = cookMateContext;
            _postRepository = postRepository;
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


        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search([FromQuery] int userId, [FromQuery] string searchString, [FromQuery] string type, [FromQuery] int? preparationTime = null)
        {
            // Initialize the response result
            var responseResult = new ResponseResult<dynamic>
            {
                IsSuccess = false,
                Message = "An error occurred."
            };

            try
            {
                // Save the search term in the searchHistory table
                var searchHistory = new SearchHistory
                {
                    UserId = userId,
                    SearchTerm = searchString,
                    SearchedAt = DateTime.UtcNow
                };
                _CookMateContext.SearchHistories.Add(searchHistory);
                await _CookMateContext.SaveChangesAsync();

                // Conduct the search
                if (type.Equals("recipe", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the search term matches any tag names exactly, case-insensitive
                    var matchingTags = await _CookMateContext.TagsLists
                    .Where(t => EF.Functions.Like(t.Name, searchString))
                    .ToListAsync();


                    var matchingTagIds = matchingTags.Select(t => t.Id).ToList();

                    // Add the matched tags to the user preferences if they don't already exist
                    foreach (var tag in matchingTags)
                    {
                        var preferenceExists = await _CookMateContext.UserPreferencesTags
                            .AnyAsync(upt => upt.UserId == userId && upt.TagId == tag.Id);

                        if (!preferenceExists)
                        {
                            _CookMateContext.UserPreferencesTags.Add(new UserPreferencesTag
                            {
                                UserId = userId,
                                TagId = tag.Id
                            });
                        }
                    }

                    // Continue with the recipe search by name and by matched tag IDs
                    var recipes = await _CookMateContext.Recipes
                        .Where(r => r.Name.Contains(searchString) ||
                                    r.RecipeTags.Any(rt => matchingTagIds.Contains(rt.TagListId)))
                        .ToListAsync();

                    await _CookMateContext.SaveChangesAsync();

                    responseResult.IsSuccess = true;
                    responseResult.Message = recipes.Any() ? "Recipes found." : "No recipes found matching your search.";
                    responseResult.Result = recipes.Select(r => new
                    {
                        RecipeId = r.Id,
                        RecipeName = r.Name,
                        RecipeMedia = r.Media,
                        // Add other recipe details as needed
                    });
                }
                else if (type.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    var users = await _CookMateContext.Users
                        .Where(u => u.Username.Contains(searchString))
                        .Select(u => new UserModel
                        {
                            Id = u.Id,
                            Username = u.Username,
                            ProfilePic = u.ProfilePic
                        })
                        .ToListAsync();

                    responseResult.IsSuccess = true;
                    responseResult.Message = "Users searched successfuly";
                    responseResult.Result = users;
                }
                else
                {
                    responseResult.Message = "Invalid search type.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                responseResult.Message = "An exception occurred while searching: " + ex.Message;
            }

            // If the operation was successful, return OK with the result, otherwise return BadRequest
            if (responseResult.IsSuccess == true)
            {
                return Ok(responseResult);
            }
            else
            {
                return BadRequest(responseResult);
            }
        }



        [HttpGet]
        [Route("getSearchHistoryByUser")]
        public async Task<IActionResult> GetSearchHistoryByUser(int userId)
        {
            var searchHistoryList = await _userRepository.GetSearchHistoryByUserIdAsync(userId);
            var responseResult = new ResponseResult<List<SearchHistory>>();

            if (searchHistoryList == null || !searchHistoryList.Any())
            {
                responseResult.IsSuccess = false;
                responseResult.Message = "No search history found for the given user.";
                return NotFound(responseResult);
            }

            responseResult.IsSuccess = true;
            responseResult.Message = "Search history retrieved successfully.";
            responseResult.Result = searchHistoryList;

            return Ok(responseResult);
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
