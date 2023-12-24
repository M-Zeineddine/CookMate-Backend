using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CookMateBackend.Controllers
{

    public class CreatePostDto
    {
        public int PostId { get; set; } // Assuming this is the input PostId
        public byte Type { get; set; }
        public int UserId { get; set; }
        public CreateRecipeDto Recipe { get; set; }
        public CreateMediaDto Media { get; set; }
    }

    public class CreateMediaDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public byte MediaType { get; set; }
        public IFormFile MediaFile { get; set; }
    }

    // DTO for creating a post along with a recipe
    public class CreatePostWithRecipeDto
    {
        public byte Type { get; set; }
        public int UserId { get; set; }
        public CreateMediaDto Media { get; set; }
        public CreateRecipeDto Recipe { get; set; }
    }

    // DTO for creating a recipe
    public class CreateRecipeDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int PreparationTime { get; set; }
        public string Media { get; set; }
        // Other properties as needed
    }


    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        public readonly CookMateContext _context;
        private readonly IPostRepository _postRepository;
        private readonly IWebHostEnvironment _environment;


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
        public async Task<ActionResult<ResponseResult<Post>>> CreatePost([FromForm] CreatePostModel model)
        {
            try
            {
                var result = await _postRepository.CreatePost(model);
                if ((bool)result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseResult<Post>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Result = null
                });
            }
        }









    }
}
