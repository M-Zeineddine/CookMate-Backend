using Azure;
using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace CookMateBackend.Controllers
{

    public class CreatePostDto
    {
        public int? PostId { get; set; } // Assuming this is the input PostId
        public byte? Type { get; set; }
        public int? UserId { get; set; }
        public CreateRecipeDto? Recipe { get; set; }
        public CreateMediaDto? Media { get; set; }
    }

    public class CreateMediaDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? MediaFile { get; set; }
    }

    // DTO for creating a post along with a recipe
    public class CreatePostWithRecipeDto
    {
        public byte? Type { get; set; }
        public int? UserId { get; set; }
        public CreateMediaDto? Media { get; set; }
        public CreateRecipeDto? Recipe { get; set; }
    }

    // DTO for creating a recipe
    public class CreateRecipeDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? PreparationTime { get; set; }
        public string? Media { get; set; }
        // Other properties as needed
    }


    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        public readonly CookMateContext _context;
        private readonly IPostRepository _postRepository;
        private readonly IWebHostEnvironment _environment;
        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";


        public PostController(CookMateContext cookMateContext, IPostRepository postRepository, IWebHostEnvironment environment)
        {
            _context = cookMateContext;
            _postRepository = postRepository;
            _environment = environment;
        }

        [HttpGet]
        [Route("getUserPostsList")]
        public async Task<ActionResult<ResponseResult<List<UserPostsModel>>>> GetPosts(int userId)
        {
            try
            {
                var recipes = await _postRepository.GetPostsByUserId(userId);

                if (recipes.IsSuccess == true)
                {
                    return Ok(recipes);
                }
                else
                {
                    return NotFound(recipes);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseResult<List<UserPostsModel>>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Result = null
                });
            }
        }

        /*[HttpPost]
        public async Task<ResponseResult<Post>> AddPost([FromForm] CreatePostDto createPostDto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Verify the post exists
                var post = await _context.Posts.FindAsync(createPostDto.PostId);
                if (post == null)
                {
                    return new ResponseResult<Post>
                    {
                        IsSuccess = false,
                        Message = "Post not found.",
                        Result = null
                    };
                }


                // If PostType is 1 and Recipe data is provided, create a recipe
                if (createPostDto.Type == 1 && createPostDto.Recipe != null)
                {
                    var recipeDto = createPostDto.Recipe;
                    var recipe = new Recipe
                    {
                        Name = recipeDto.Name,
                        Description = recipeDto.Description,
                        PreperationTime = recipeDto.PreparationTime,
                        Media = "dsfdf"
                        // Other properties from recipeDto...
                    };

                    _context.Recipes.Add(recipe);
                    await _context.SaveChangesAsync(); // This generates the Recipe ID

                    // Associate the recipe with the post
                    post.RecipeId = recipe.Id;
                }



                // Handle the media upload if Media is provided
                if (createPostDto.Media != null)
                {
                    var mediaDto = createPostDto.Media;
                    var mediaData = ConvertToBytes(mediaDto.MediaFile);
                    var newMedia = new Media
                    {
                        Title = mediaDto.Title,
                        Description = mediaDto.Description,
                        MediaType = mediaDto.MediaType,
                        MediaData = mediaData,
                        // If the Media entity has a PostId, set it here
                    };

                    _context.Media.Add(newMedia);
                }

                // If the Recipe is created or Media is added, update the Post
                _context.Posts.Update(post);
                await _context.SaveChangesAsync(); // Save changes for Post, Recipe, and Media
                await transaction.CommitAsync();

                return new ResponseResult<Post>
                {
                    IsSuccess = true,
                    Message = "Post updated, and optionally recipe and media added successfully.",
                    Result = post
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ResponseResult<Post>
                {
                    IsSuccess = false,
                    Message = $"Error adding post, recipe, and media: {ex.Message}",
                    Result = null
                };
            }
        }*/


        [HttpPost]
        public async Task<ResponseResult<Post>> CreatePost([FromForm] CreatePostModel model)
        {
            var result = await _postRepository.CreatePost(model);
            return result;
        }


        [HttpGet]
        [Route("getRecipeDetails")]
        public async Task<ActionResult<ResponseResult<RecipeDetailsModel>>> GetRecipeDetails(int id, int loggedInUserId)
        {
            var recipeDetails = await _postRepository.GetRecipeDetailsByIdAsync(id, loggedInUserId);
            if (recipeDetails == null)
            {
                return NotFound(new ResponseResult<RecipeDetailsModel>
                {
                    IsSuccess = false,
                    Message = "Recipe not found.",
                    Result = null
                });
            }

            return Ok(new ResponseResult<RecipeDetailsModel>
            {
                IsSuccess = true,
                Message = "Recipe details retrieved successfully.",
                Result = recipeDetails
            });
        }

        [HttpGet]
        [Route("getRecipeFeedForUser")]
        public async Task<ActionResult<ResponseResult<List<RecipeDto>>>> GetRecipeFeedForUser(int userId)
        {
            return await _postRepository.GetRecipeFeedForUserAsync(userId);
        }
        
        [HttpGet]
        [Route("getMediaFeedForUser")]
        public async Task<ActionResult<ResponseResult<List<MediaDto>>>> GetMediaFeedForUser(int userId)
        {
            return await _postRepository.GetMediaFeedForUserAsync(userId);
        }

        [HttpGet]
        [Route("searchRecipes")]
        public async Task<ActionResult<ResponseResult<List<RecipeSearchResultModel>>>> SearchRecipes([FromQuery] string searchString = "")
        {
            ResponseResult<List<RecipeSearchResultModel>> responseResult = await _postRepository.SearchRecipesAsync(searchString);
            return responseResult;
        }
        
        [HttpGet]
        [Route("searchTags")]
        public async Task<ActionResult<ResponseResult<List<TagsList>>>> SearchTags([FromQuery] string searchString = "")
        {
            ResponseResult<List<TagsList>> responseResult = await _postRepository.SearchTagsAsync(searchString);
            return responseResult;
        }

        [HttpGet]
        [Route("searchAutofill")]
        public async Task<ActionResult<ResponseResult<List<dynamic>>>> SearchAutofill([FromQuery] string searchString = "")
        {
            try
            {
                var tagsQuery = _context.TagsLists
                    .Where(t => t.Name.Contains(searchString))
                    .Take(3)
                    .Select(t => new SearchResultItem
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Type = 0,
                        TagCategoryId = t.TagCategoryId
                    });

                var recipesQuery = _context.Recipes
                    .Where(r => r.Name.Contains(searchString))
                    .Take(3)
                    .Select(r => new SearchResultItem
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Type = 1,
                        TagCategoryId = null // Explicitly nullable
                    });

                var usersQuery = _context.Users
                    .Where(u => u.Username.Contains(searchString))
                    .Take(3)
                    .Select(u => new SearchResultItem
                    {
                        Id = u.Id,
                        Name = u.Username,
                        Type = 2,
                        TagCategoryId = null // Explicitly nullable
                    });


                var combinedResults = await tagsQuery
                    .Concat(recipesQuery)
                    .Concat(usersQuery)
                    .ToListAsync();

                        // If no results, return a message indicating no matches were found
                        if (!combinedResults.Any())
                {
                    return Ok(new ResponseResult<List<dynamic>>
                    {
                        IsSuccess = false,
                        Message = "No matches found.",
                        Result = new List<dynamic>()
                    });
                }

                // If results are found, return them with a success message
                return Ok(new ResponseResult<List<SearchResultItem>>
                {
                    IsSuccess = combinedResults.Any(),
                    Message = combinedResults.Any() ? "Autofill search results retrieved successfully." : "No matches found.",
                    Result = combinedResults
                });
            }
            catch (Exception ex)
            {
                // If there's an exception, return an error response with the exception message
                return StatusCode(500, new ResponseResult<List<dynamic>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}",
                    Result = null
                });
            }
        }


        [HttpPost]
        [Route("addRecipeTag")]
        public async Task<ResponseResult<List<RecipeTag>>> AddRecipeTagsAsync(List<CreateRecipeTagModel> tagModels)
        {
            ResponseResult<List<RecipeTag>> responseResult = await _postRepository.AddRecipeTagsAsync(tagModels);
            return responseResult;
        }


        /*[HttpPost]
        [Route("addRecipeTag")]
        public async Task<ResponseResult<List<int>>> AddRecipeTags(int recipeId, int[] tagIds)
        {
            ResponseResult<List<int>> responseResult = await _postRepository.AddRecipeTagsAsync(recipeId, tagIds);
            return responseResult;
        }*/

        /*[HttpPost]
        [Route("addProcedure")]
        public async Task<ActionResult<ResponseResult<List<RecipeSearchResultModel>>>> SearchRecipes([FromQuery] string searchString = "")
        {
            ResponseResult<List<RecipeSearchResultModel>> responseResult = await _postRepository.SearchRecipesAsync(searchString);
            return responseResult;
        }*/


        [HttpGet]
        [Route("generalSearch")]
        public async Task<ActionResult<ResponseResult<dynamic>>> GeneralSearch([FromQuery] string searchString = "",
                                                  [FromQuery] string sortBy = null,
                                                  [FromQuery] int? postAt = null,
                                                  [FromQuery] string prepTime = null,
                                                  [FromQuery] int? rate = null,
                                                  [FromQuery] List<int> tags = null,
                                                  [FromQuery] int type = -1)
        {
            string recipeMediaPath = "uploads/recipes/";
            var response = new ResponseResult<dynamic>();


            try
            {
                //Recipe Query
                var query = _context.Recipes.AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(r => r.Name.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(prepTime))
                {
                    query = query.Where(r => string.Compare(r.PreperationTime, prepTime) <= 0);
                }

                if (rate.HasValue)
                {
                    query = query.Where(r => r.Reviews.Any() && r.Reviews.Average(review => review.Rating) >= rate.Value);
                }

                if (postAt.HasValue)
                {
                    var dateFrom = DateTime.Now.AddDays(-postAt.Value);
                    query = query.Where(r => r.CreatedAt >= dateFrom);
                }
                if (tags != null && tags.Count > 0)
                {
                    query = query.Where(r => r.RecipeTags.Any(rt => tags.Contains(rt.TagListId)));
                }

                // Apply dynamic sorting
                switch (sortBy)
                {
                    case "name":
                        query = query.OrderBy(r => r.Name);
                        break;
                    case "prepTime":
                        query = query.OrderBy(r => r.PreperationTime);
                        break;
                    case "rate":
                        query = query.OrderByDescending(r => r.Reviews.Any() ? r.Reviews.Average(review => review.Rating) : 0);
                        break;
                    case "postAt":
                        query = query.OrderByDescending(r => r.CreatedAt);
                        break;
                    case "createdAt": // This is the new case for sorting by CreatedAt
                        query = query.OrderBy(r => r.CreatedAt);
                        break;
                        // Add more cases for other sort options as needed
                }

                var finalQuery = query.Select(recipe => new RecipeDto
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    Description = recipe.Description,
                    PreparationTime = recipe.PreperationTime,
                    Media = !string.IsNullOrEmpty(recipe.Media) ? $"{baseUrl}{recipeMediaPath}{recipe.Media}" : null,
                    AverageRating = recipe.Reviews.Any() ? recipe.Reviews.Average(r => r.Rating) : 0,
                    CreatedAt = recipe.CreatedAt,
                    User = new UserModel // Populate this according to your UserModel structure
                    {
                        // Assuming you have logic to populate this based on your recipe's user
                    }
                });

                //Users Query
                var recipeResults = await finalQuery.ToListAsync();

                // Search for users with the same searchString without applying sorting and filtering
                var userResults = await _context.Users
                            .Where(u => string.IsNullOrEmpty(searchString) || u.Username.Contains(searchString) || u.Email.Contains(searchString))
                            .Select(u => new UserModel
                            {
                                Id = u.Id,
                                Username = u.Username,
                                ProfilePic = u.ProfilePic // Assuming ProfilePic is directly mapped
                            })
                            .ToListAsync();

                response.IsSuccess = true;
                response.Message = "Search results fetched successfully.";
                response.Result = new { Recipes = recipeResults, Users = userResults };
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"An error occurred: {ex.Message}";
                // Optionally include more detailed error information here
            }
            return Ok(response);
        }


    }
}
