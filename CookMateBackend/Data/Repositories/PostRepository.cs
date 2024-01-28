using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";

        public async Task<ResponseResult<List<UserPostsModel>>> GetPostsByUserId(int userId)
        {
            var result = new ResponseResult<List<UserPostsModel>>();

            try
            {
                // Define the base URL for media access based on your server's address
                
                // Specify the paths for recipes and general media if they are different
                string recipeMediaPath = "uploads/recipes/";
                string generalMediaPath = "uploads/media/";

                var allRecipes = await _CookMateContext.Recipes.ToListAsync();

                // Include both Recipe and Media relationships in the query
                var query = _CookMateContext.Posts
                            .Include(p => p.Recipe)
                            .Include(p => p.Media)
                            .Where(p => p.UserId == userId);

                var postsWithDetails = await query.ToListAsync();

                var userPosts = postsWithDetails
                .Select(p => new
                {
                    Post = p,
                    RecipeAvgRating = p.Recipe != null ? _CookMateContext.Reviews.Where(r => r.RecipesId == p.Recipe.Id).Average(r => (decimal?)r.Rating) : null,
                    LatestDate = (p.Recipe != null && p.Media != null)
                                 ? (p.Recipe.CreatedAt > p.Media.CreatedAt ? p.Recipe.CreatedAt : p.Media.CreatedAt)
                                 : (p.Recipe?.CreatedAt ?? p.Media?.CreatedAt)
                })
                .OrderByDescending(p => p.LatestDate) // Sorting by the latest date
                .Select(p => new UserPostsModel
                {
                    Id = p.Post.Id,
                    PostType = p.Post.Type, // Make sure this is cast correctly if needed
                    Recipe = p.Post.Recipe != null ? new RecipeDto // Only populate if Recipe is not null
                    {
                        Id = p.Post.Recipe.Id,
                        Name = p.Post.Recipe.Name,
                        Description = p.Post.Recipe.Description,
                        PreparationTime = p.Post.Recipe.PreperationTime,
                        Media = p.Post.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{p.Post.Recipe.Media}" : null,
                        CreatedAt = p.Post.Recipe.CreatedAt,
                        AverageRating = p.RecipeAvgRating ?? 0

                        // ... other RecipeDto properties
                    } : null,
                    Media = p.Post.Media != null ? new MediaDto // Only populate if Media is not null
                    {
                        Id = p.Post.Media.Id,
                        Title = p.Post.Media.Title,
                        Description = p.Post.Media.Description,
                        MediaData = p.Post.Media.MediaData != null ? $"{baseUrl}{generalMediaPath}{p.Post.Media.MediaData}" : null,
                        CreatedAt = p.Post.Media.CreatedAt,
                        // ... other MediaDto properties
                        RecipeReference = p.Post.Media.RecipeId.HasValue ? new RecipeReferenceDto
                        {
                            Id = p.Post.Media.RecipeId.Value,
                            Name = allRecipes.FirstOrDefault(r => r.Id == p.Post.Media.RecipeId.Value)?.Name
                        } : null
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

            // Validate the input data
            if (model.UserId == 0 || model.Type == 0 || (model.Type == 1 && string.IsNullOrEmpty(model.RecipeName)) || (model.Type == 2 && string.IsNullOrEmpty(model.MediaTitle)))
            {
                result.IsSuccess = false;
                result.Message = "Missing input: " + (model.UserId == 0 ? "User ID, " : "") + (model.Type == 0 ? "Post Type, " : "") + (model.Type == 1 && string.IsNullOrEmpty(model.RecipeName) ? "Recipe Name, " : "") + (model.Type == 2 && string.IsNullOrEmpty(model.MediaTitle) ? "Media Title, " : "");
                result.Message = result.Message.TrimEnd(' ', ',');
                return result;
            }

            using (var transaction = await _CookMateContext.Database.BeginTransactionAsync())
            {
                try
                {

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                    string? filePath = null;
                    string? uniqueFileName = null; // Store just the file name

                    if ((model.Type == 1 && model.RecipeMedia != null) || (model.Type == 2 && model.MediaData != null))
                    {
                        IFormFile? file = model.Type == 1 ? model.RecipeMedia : model.MediaData;
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
                        bool recipeExists = _CookMateContext.Recipes
                    .Join(_CookMateContext.Posts, // Join with Posts
                          recipe => recipe.Id, // from Recipe
                          post => post.RecipeId, // to Post
                          (recipe, post) => new { Recipe = recipe, Post = post }) // select both
                    .Any(joined => joined.Recipe.Name.ToLower() == model.RecipeName.ToLower()
                                    && joined.Post.UserId == model.UserId
                                    && joined.Post.Type == 1);

                        if (recipeExists)
                        {
                            // Return an error message if the recipe name is already in use
                            result.IsSuccess = false;
                            result.Message = "You already have a recipe with this name.";
                            return result;
                        }

                        // Save recipe information
                        Recipe newRecipe = new Recipe
                        {
                            Name = model.RecipeName,
                            Description = model.RecipeDescription,
                            PreperationTime = model.PreparationTime.HasValue ? model.PreparationTime.Value.ToString() : null,
                            Media = uniqueFileName, // Save just the file name
                            CreatedAt = DateTime.Now // Add the current date and time
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
                            MediaData = uniqueFileName,
                            CreatedAt = DateTime.Now,
                            RecipeId = model.RecipeId
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
                    result.Result = newPost;
                    result.Message = newPost.RecipeId.ToString();
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


        public async Task<RecipeDetailsModel> GetRecipeDetailsByIdAsync(int recipeId, int loggedInUserId)
        {
            // Define the base URL for media access based on your server's address
            string recipeMediaPath = "uploads/recipes/";
            string ingredientMediaPath = "uploads/ingredients/";

            var recipe = await _CookMateContext.Recipes
                .Where(r => r.Id == recipeId)
                .Select(r => new RecipeDetailsModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    PreparationTime = r.PreperationTime,
                    Media = !string.IsNullOrEmpty(r.Media) ? $"{baseUrl}{recipeMediaPath}{r.Media}" : null,
                    IsCreatedByUser = r.Posts.Any(p => p.UserId == loggedInUserId && p.RecipeId == r.Id),
                    Procedures = r.Procedures.Select(p => new Procedure
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Media = !string.IsNullOrEmpty(p.Media) ? $"{baseUrl}{recipeMediaPath}{p.Media}" : null,
                        Time = p.Time,
                        Step = p.Step,
                        RecipeId = p.RecipeId
                    }).ToList(),
                    Ingredients = r.RecipeIngredients.Select(ri => new IngredientDto
                    {
                        Id = ri.IngredientList.Id,
                        Name = ri.IngredientList.Name,
                        Weight = ri.Weight,
                        MediaUrl = !string.IsNullOrEmpty(ri.IngredientList.Media) ? $"{baseUrl}{ingredientMediaPath}{ri.IngredientList.Media}" : null
                    }).ToList(),
                    Tags = r.RecipeTags
                    .Select(rt => new TagDto
                    {
                        Id = rt.TagList.Id,
                        Name = rt.TagList.Name
                        // Map other properties as needed.
                    }).ToList(),
                    User = r.Posts
                        .Where(p => p.Type == 1 && p.RecipeId == r.Id)
                        .Select(p => new UserModel
                        {
                            Id = p.User.Id,
                            Username = p.User.Username,
                            ProfilePic = p.User.ProfilePic
                            // Map additional user properties as needed.
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            return recipe;
        }


        public async Task<ResponseResult<List<RecipeSearchResultModel>>> SearchRecipesAsync(string searchString)
        {
            List<RecipeSearchResultModel> searchResults;

            if (string.IsNullOrEmpty(searchString))
            {
                // If the search string is null or empty, return all recipes
                searchResults = await _CookMateContext.Recipes
                    .Select(r => new RecipeSearchResultModel
                    {
                        RecipeId = r.Id,
                        RecipeName = r.Name,
                        OwnerUsername = r.Posts
                            .Where(p => p.Type == 1 && p.RecipeId == r.Id)
                            .Select(p => p.User.Username)
                            .FirstOrDefault() // Assuming one owner per recipe
                    }).ToListAsync();
            }
            else
            {
                // If there's a search string, return recipes that match the condition
                searchResults = await _CookMateContext.Recipes
                    .Where(r => r.Name.Contains(searchString)) // Modify this condition as needed
                    .Select(r => new RecipeSearchResultModel
                    {
                        RecipeId = r.Id,
                        RecipeName = r.Name,
                        OwnerUsername = r.Posts
                            .Where(p => p.Type == 1 && p.RecipeId == r.Id)
                            .Select(p => p.User.Username)
                            .FirstOrDefault() // Assuming one owner per recipe
                    }).ToListAsync();
            }

            // Wrap in a ResponseResult and return
            return new ResponseResult<List<RecipeSearchResultModel>>
            {
                IsSuccess = searchResults.Any(),
                Message = searchResults.Any() ? "Recipes found." : "No recipes found.",
                Result = searchResults
            };
        }


        public async Task<ResponseResult<List<TagsList>>> SearchTagsAsync(string searchString)
        {
            List<TagsList> searchResults;

            if (string.IsNullOrEmpty(searchString))
            {
                // If the search string is null or empty, return all tags
                searchResults = await _CookMateContext.TagsLists
                    .Select(t => new TagsList
                    {
                        Id = t.Id,
                        Name = t.Name,
                        // You can add more fields from the tag as needed, such as the tag category
                        TagCategoryId = t.TagCategoryId
                    }).ToListAsync();
            }
            else
            {
                // If there's a search string, return tags that match the condition
                searchResults = await _CookMateContext.TagsLists
                    .Where(t => t.Name.Contains(searchString)) // Modify this condition as needed
                    .Select(t => new TagsList
                    {
                        Id = t.Id,
                        Name = t.Name,
                        // You can add more fields from the tag as needed, such as the tag category
                        TagCategoryId = t.TagCategoryId
                    }).ToListAsync();
            }

            // Wrap in a ResponseResult and return
            return new ResponseResult<List<TagsList>>
            {
                IsSuccess = searchResults.Any(),
                Message = searchResults.Any() ? "Tags found." : "No tags found.",
                Result = searchResults
            };
        }






        public async Task<ResponseResult<List<RecipeTag>>> AddRecipeTagsAsync(List<CreateRecipeTagModel> tagModels)
        {
            var result = new ResponseResult<List<RecipeTag>>();

            try
            {
                List<RecipeTag> newTags = new List<RecipeTag>();

                foreach (var tagModel in tagModels.DistinctBy(tm => new { tm.RecipeId, tm.TagListId }))
                {
                    bool tagExists = await _CookMateContext.RecipeTags
                        .AnyAsync(rt => rt.RecipeId == tagModel.RecipeId && rt.TagListId == tagModel.TagListId);

                    if (!tagExists)
                    {
                        RecipeTag newTag = new RecipeTag
                        {
                            RecipeId = tagModel.RecipeId,
                            TagListId = tagModel.TagListId
                        };

                        _CookMateContext.RecipeTags.Add(newTag);
                        newTags.Add(newTag); // It's better to add after context.Add to avoid any side effects

                        result.IsSuccess = true;
                        result.Message = "Tag added successfuly";
                        result.Result = newTags;
                    }
                    else
                    {
                        result.IsSuccess = true;
                        result.Message = "Couldn't add tag";
                        result.Result = null;

                    }
                }

                await _CookMateContext.SaveChangesAsync();

                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Couldn't add tag";
                result.Result = null;
            }

            return result;
        }



        public async Task<ResponseResult<List<RecipeDto>>> GetRecipeFeedForUserAsync(int loggedInUserId)
        {
            var result = new ResponseResult<List<RecipeDto>>();

            try
            {
                // Define the base URL for profile picture access based on your server's address
                string profilePicPath = "path/to/profile_pics/"; // Adjust as necessary
                string recipeMediaPath = "uploads/recipes/";

                // Get the IDs of the users that the logged-in user follows
                var followedUserIds = await _CookMateContext.Followers
                    .Where(f => f.FollowerId == loggedInUserId)
                    .Select(f => f.UserId)
                    .ToListAsync();

                // Get the posts with type 1 from the followed users
                var recipePosts = await _CookMateContext.Posts
                    .Where(p => followedUserIds.Contains(p.UserId) && p.Type == 1 && p.Recipe != null)
                    .Include(p => p.Recipe)
                    .ThenInclude(r => r.Reviews) // Include the Reviews for each Recipe
                    .Include(p => p.User)
                    .OrderByDescending(p => p.Recipe.CreatedAt)
                    .Select(p => new RecipeDto
                    {
                        Id = p.Recipe.Id,
                        Name = p.Recipe.Name,
                        Description = p.Recipe.Description,
                        PreparationTime = p.Recipe.PreperationTime,
                        Media = !string.IsNullOrEmpty(p.Recipe.Media) ? $"{baseUrl}{recipeMediaPath}{p.Recipe.Media}" : null,
                        CreatedAt = p.Recipe.CreatedAt,
                        User = new UserModel
                        {
                            Id = p.UserId,
                            Username = p.User.Username,
                            ProfilePic = !string.IsNullOrEmpty(p.User.ProfilePic)
                                         ? $"{baseUrl}{profilePicPath}{p.User.ProfilePic}"
                                         : null
                        },
                        AverageRating = p.Recipe.Reviews.Any() ? p.Recipe.Reviews.Average(r => r.Rating) : 0 // Calculate average rating
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Message = "Recipe feed fetched successfully.";
                result.Result = recipePosts;
            }
            catch (Exception)
            {
                // Log the exception
                result.IsSuccess = false;
                result.Message = "An error occurred while fetching the recipe feed.";
                // In production, do not expose the exception details
                // result.Message = ex.Message; 
                result.Result = null;
            }

            return result;
        }



        public async Task<ResponseResult<List<MediaDto>>> GetMediaFeedForUserAsync(int loggedInUserId)
        {
            var result = new ResponseResult<List<MediaDto>>();

            try
            {
                // Define the base URL for profile picture access based on your server's address
                string profilePicPath = "path/to/profile_pics/"; // Adjust as necessary
                string generalMediaPath = "uploads/media/";

                // Get the IDs of the users that the logged-in user follows
                var followedUserIds = await _CookMateContext.Followers
                    .Where(f => f.FollowerId == loggedInUserId)
                    .Select(f => f.UserId)
                    .ToListAsync();

                var mediaPosts = await _CookMateContext.Posts
                    .Where(p => followedUserIds.Contains(p.UserId) && p.Type == 2 && p.Media != null)
                    .Include(p => p.Media)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.Media.CreatedAt) // Sorting by the latest recipes first
                    .Select(p => new MediaDto
                    {
                        Id = p.Media.Id,
                        Title = p.Media.Title,
                        Description = p.Media.Description,
                        MediaData = !string.IsNullOrEmpty(p.Media.MediaData) ? $"{baseUrl}{generalMediaPath}{p.Media.MediaData}" : null,
                        Likes = p.Media.Likes,
                        CreatedAt = p.Media.CreatedAt, // Assuming CreatedAt is not nullable
                        User = new UserModel
                        {
                            Id = p.User.Id,
                            Username = p.User.Username,
                            ProfilePic = !string.IsNullOrEmpty(p.User.ProfilePic)
                                         ? $"{profilePicPath}{p.User.ProfilePic}"
                                         : null
                        },
                        FavoritesCount = _CookMateContext.Favorites.Count(f => f.PostId == p.Id), // Count the favorites for this post
                        RecipeReference = p.Media.RecipeId.HasValue ? new RecipeReferenceDto
                        {
                            Id = p.Media.RecipeId.Value,

                            Name = _CookMateContext.Recipes.Where(r => r.Id == p.Media.RecipeId.Value).Select(r => r.Name).FirstOrDefault()
                        } : null

                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Message = "Media feed fetched successfully.";
                result.Result = mediaPosts;
            }
            catch (Exception ex)
            {
                // Log the exception
                result.IsSuccess = false;
                result.Message = "An error occurred while fetching the media feed.";
                // In production, do not expose the exception details
                // result.Message = ex.Message; 
                result.Result = null;
            }

            return result;
        }



        public async Task<bool> UpdateMediaLikesAsync(int mediaId, bool addLike)
        {
            try
            {
                // Retrieve the media entity from the database
                var media = await _CookMateContext.Media
                    .FirstOrDefaultAsync(m => m.Id == mediaId);

                // Check if the media exists
                if (media == null)
                {
                    return false; // Or handle the error as needed
                }

                // Update the likes count based on the addLike flag
                // Assuming 'Likes' is an integer. If it's a string representing an integer, convert accordingly.
                if (addLike)
                {
                    media.Likes += 1;
                }
                else
                {
                    // Ensure that we don't decrement below zero
                    media.Likes = media.Likes > 0 ? media.Likes - 1 : 0;
                }

                // Save the changes back to the database
                await _CookMateContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception and handle it appropriately
                // For example:
                // _logger.LogError("An error occurred when updating media likes: {Exception}", ex);
                return false;
            }
        }


    }





}
