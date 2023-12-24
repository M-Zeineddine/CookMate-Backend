using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace CookMateBackend.Data.Repositories
{
    public class PostRepository: IPostRepository
    {
        public readonly CookMateContext _CookMateContext;

        public PostRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }

        public async Task<ResponseResult<List<UserPostsModel>>> GetPostsByUserId(int userId)
        {
            var result = new ResponseResult<List<UserPostsModel>>();

            try
            {
                // Define the base URL for media access based on your server's address
                string baseUrl = "http://192.168.1.110:5000/";

                // Specify the paths for recipes and general media if they are different
                string recipeMediaPath = "uploads/recipes/";
                string generalMediaPath = "uploads/media/";

                // Include both Recipe and Media relationships in the query
                var query = _CookMateContext.Posts
                            .Include(p => p.Recipe)
                            .Include(p => p.Media)
                            .Where(p => p.UserId == userId);

                var postsWithDetails = await query.ToListAsync();

                var userPosts = postsWithDetails
                    .Select(p => new UserPostsModel
                    {
                        Id = p.Id,
                        PostType = p.Type,
                        Recipe = p.Recipe != null ? new RecipeDto // Only populate if Recipe is not null
                        {
                            Id = p.Recipe.Id,
                            Name = p.Recipe.Name,
                            Description = p.Recipe.Description,
                            PreparationTime = p.Recipe.PreperationTime,
                            Media = p.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{p.Recipe.Media}" : null
                            // Map other recipe properties
                        } : null,
                        Media = p.Media != null ? new MediaDto // Only populate if Media is not null
                        {
                            Id = p.Media.Id,
                            Title = p.Media.Title,
                            Description = p.Media.Description,
                            MediaType = p.Media.MediaType,
                            MediaData = p.Media.MediaData != null ? $"{baseUrl}{generalMediaPath}{p.Media.MediaData}" : null
                            // Map other media properties
                        } : null
                    }).ToList();

                result.IsSuccess = true;
                result.Message = "Posts fetched successfully";
                result.Result = userPosts;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message; // Consider logging this instead of sending it to the client.
                result.Result = null;
            }

            return result;
        }


        public async Task<ResponseResult<Post>> CreatePost(CreatePostModel model)
        {
            var result = new ResponseResult<Post>();
            using (var transaction = await _CookMateContext.Database.BeginTransactionAsync())
            {
                try
                {

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                    string filePath = null;
                    string uniqueFileName = null; // Store just the file name

                    if ((model.Type == 1 && model.RecipeMedia != null) || (model.Type == 2 && model.MediaData != null))
                    {
                        IFormFile file = model.Type == 1 ? model.RecipeMedia : model.MediaData;
                        string folderPath = model.Type == 1 ? Path.Combine(uploadsFolder, "recipes") : Path.Combine(uploadsFolder, "media");
                        Directory.CreateDirectory(folderPath); // Ensure the directory exists

                        uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName; // Unique file name
                        filePath = Path.Combine(folderPath, uniqueFileName); // Full path used for saving the file

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
                            Media = uniqueFileName // Save just the file name
                        };
                        _CookMateContext.Recipes.Add(newRecipe);
                        await _CookMateContext.SaveChangesAsync();

                        // Link the RecipeId to the Post
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
                            MediaData = uniqueFileName // Save just the file name
                        };
                        _CookMateContext.Media.Add(newMedia);
                        await _CookMateContext.SaveChangesAsync();

                        // Link the MediaId to the Post
                        newPost.MediaId = newMedia.Id;
                    }

                    _CookMateContext.Posts.Add(newPost);
                    await _CookMateContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    result.IsSuccess = true;
                    result.Result = newPost; // Assuming newPost is the post you've created and saved
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

    }
}
