using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Data.Repositories
{
    public class FavoriteRepository : IFavoritesRepository
    {
        private readonly CookMateContext _context;
        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";

        public FavoriteRepository(CookMateContext context)
        {
            _context = context;
        }

        public async Task<ResponseResult<bool>> ToggleFavorite(FavoriteModel favoriteModel)
        {
            var result = new ResponseResult<bool>();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Check if the favorite is for a recipe or media and get the corresponding post
                    var postQuery = _context.Posts.AsQueryable();
                    if (favoriteModel.RecipeId.HasValue)
                    {
                        postQuery = postQuery.Where(p => p.RecipeId == favoriteModel.RecipeId.Value && p.Type == 1); // Access .Value here because HasValue is true
                    }
                    else if (favoriteModel.MediaId.HasValue)
                    {
                        postQuery = postQuery.Where(p => p.MediaId == favoriteModel.MediaId.Value && p.Type == 2); // Access .Value here because HasValue is true
                    }


                    var post = await postQuery.FirstOrDefaultAsync();

                    if (post == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "Post not found for the given ID.";
                        return result;
                    }

                    // Check if the post is already favorited by the user
                    var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == favoriteModel.UserId && f.PostId == post.Id);

                    if (favorite != null)
                    {
                        // Remove the favorite and the corresponding like
                        var like = await _context.RecipeLikes
                            .Include(l => l.Interaction) // Eagerly load the Interaction
                            .FirstOrDefaultAsync(l => l.Interaction.UserId == favoriteModel.UserId && l.RecipeId == post.RecipeId);

                        if (like != null)
                        {
                            _context.RecipeLikes.Remove(like);
                            // Now we can be sure that like.Interaction is not null
                            _context.InteractionHistories.Remove(like.Interaction);
                        }

                        _context.Favorites.Remove(favorite);
                        result.Message = "Removed from favorites and likes.";
                    }
                    else
                    {
                        // Add the favorite and the corresponding like
                        var interaction = new InteractionHistory { UserId = favoriteModel.UserId, CreatedAt = DateTime.UtcNow };
                        _context.InteractionHistories.Add(interaction);
                        await _context.SaveChangesAsync();  // Save to get the InteractionId for the like

                        favorite = new Favorite { UserId = favoriteModel.UserId, PostId = post.Id };
                        _context.Favorites.Add(favorite);

                        if (post.RecipeId.HasValue)
                        {
                            var newLike = new RecipeLike
                            {
                                InteractionId = interaction.InteractionId,
                                RecipeId = post.RecipeId.Value, // Safe to use .Value because we checked HasValue above
                                LikedAt = DateTime.UtcNow
                            };
                            _context.RecipeLikes.Add(newLike);
                        }
                        result.Message = "Added to favorites";
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    result.IsSuccess = true;
                    result.Result = true;
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


        /*private async Task<int?> GetPostIdFromMediaOrRecipe(int? mediaId, int? recipeId)
        {
            if (mediaId.HasValue)
            {
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.MediaId == mediaId && p.Type == 2);
                return post?.Id;
            }
            else if (recipeId.HasValue)
            {
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.RecipeId == recipeId && p.Type == 1);
                return post?.Id;
            }

            return null; // No valid ID provided
        }*/


        public async Task<ResponseResult<List<UserPostsModel>>> GetFavorites(int userId, string favoriteType = null)
        {
            string recipeMediaPath = "uploads/recipes/";
            string generalMediaPath = "uploads/media/";

            var result = new ResponseResult<List<UserPostsModel>>();
            try
            {
                var favoritesQuery = _context.Favorites
                    .Where(f => f.UserId == userId);

                if (!string.IsNullOrEmpty(favoriteType))
                {
                    if (favoriteType.Equals("media", StringComparison.OrdinalIgnoreCase))
                    {
                        favoritesQuery = favoritesQuery.Where(f => f.Post.Type == 2);
                    }
                    else if (favoriteType.Equals("recipe", StringComparison.OrdinalIgnoreCase))
                    {
                        favoritesQuery = favoritesQuery.Where(f => f.Post.Type == 1);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid favorite type specified.");
                    }
                }

                var favorites = await favoritesQuery
                    .Select(f => new UserPostsModel
                    {
                        Id = f.Post.Id,
                        PostType = f.Post.Type,
                        Media = f.Post.Type == 2 ? new MediaDto
                        {
                            Id = f.Post.MediaId,
                            Title = f.Post.Media.Title,
                            Description = f.Post.Media.Description,
                            MediaData = f.Post.Media.MediaData != null ? $"{baseUrl}{generalMediaPath}{f.Post.Media.MediaData}" : null,
                            Likes = f.Post.Media.Likes,
                            CreatedAt = f.Post.Media.CreatedAt,
                            RecipeReference = f.Post.Media.RecipeId.HasValue ? new RecipeReferenceDto
                            {
                                Id = f.Post.Media.RecipeId,
                                Name = f.Post.Media.Recipe.Name
                            } : null
                        } : null,
                        Recipe = f.Post.Type == 1 ? new RecipeDto
                        {
                            Id = f.Post.RecipeId,
                            Name = f.Post.Recipe.Name,
                            Description = f.Post.Recipe.Description,
                            PreparationTime = f.Post.Recipe.PreperationTime,
                            Media = f.Post.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{f.Post.Recipe.Media}" : null,
                            CreatedAt = f.Post.Recipe.CreatedAt,
                            AverageRating = f.Post.Recipe.Reviews.Any() ? f.Post.Recipe.Reviews.Average(r => r.Rating) : 0
                        } : null
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Result = favorites;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }

    }

}
