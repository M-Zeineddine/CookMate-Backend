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

        private byte[] ConvertToBytes(IFormFile mediaFile)
        {
            if (mediaFile == null) return null;

            using (var memoryStream = new MemoryStream())
            {
                mediaFile.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        [HttpGet]
        [Route("getUserPostsList")]
        public async Task<ActionResult<ResponseResult<List<UserPostsModel>>>> GetPosts(int userId, int postType)
        {
            try
            {
                var recipes = await _postRepository.GetPostsByUserId(userId, postType);

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
            var result = new ResponseResult<Post>();

            // Assuming _context is your database context
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Handle file upload
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                    string filePath = null;

                    if ((model.Type == 1 && model.RecipeMedia != null) || (model.Type == 2 && model.MediaData != null))
                    {
                        IFormFile file = model.Type == 1 ? model.RecipeMedia : model.MediaData;
                        string folderPath = model.Type == 1 ? Path.Combine(uploadsFolder, "recipes") : Path.Combine(uploadsFolder, "media");
                        Directory.CreateDirectory(folderPath); // Ensure the directory exists

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        filePath = Path.Combine(folderPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }

                    // Create and save the new post with media
                    Post newPost = new Post
                    {
                        UserId = model.UserId,
                        Type = model.Type
                        // Populate other fields as necessary
                    };

                    if (model.Type == 1)
                    {
                        // Save recipe information
                        Recipe newRecipe = new Recipe
                        {
                            Name = model.RecipeName,
                            Description = model.RecipeDescription,
                            PreperationTime = model.PreparationTime.HasValue ? model.PreparationTime.Value.ToString() : null,
                            Media = filePath // Assuming this is the correct property for a Recipe entity
                        };
                        _context.Recipes.Add(newRecipe);
                        await _context.SaveChangesAsync();

                        // Assuming you want to link the RecipeId to the Post
                        newPost.RecipeId = newRecipe.Id;
                    }
                    else if (model.Type == 2)
                    {
                        // Save media information
                        Media newMedia = new Media
                        {
                            Title = model.MediaTitle,
                            Description = model.MediaDescription,
                            MediaType = (byte)model.MediaType,
                            MediaData = filePath
                        };
                        _context.Media.Add(newMedia);
                        await _context.SaveChangesAsync();

                        // Assuming you want to link the MediaId to the Post
                        newPost.MediaId = newMedia.Id;
                    }

                    _context.Posts.Add(newPost);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    result.IsSuccess = true;
                    result.Result = newPost;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.IsSuccess = false;
                    result.Message = ex.Message;
                }
            }

            return result;
        }



        private async Task<string> SaveFileAsync(IFormFile file, string uploadsFolderPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty.", nameof(file));
            }

            if (string.IsNullOrEmpty(uploadsFolderPath))
            {
                throw new ArgumentException("Uploads folder path cannot be null or empty.", nameof(uploadsFolderPath));
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filePath; // Or return a relative path or URL as needed
        }







    }
}
