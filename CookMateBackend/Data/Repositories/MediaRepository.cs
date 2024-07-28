using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Data.Interfaces;

namespace CookMateBackend.Data.Repositories
{
    public class MediaRepository: IMediaRepository
    {

        public readonly CookMateContext _context;

        public MediaRepository(CookMateContext cookMateContext)
        {
            _context = cookMateContext;
        }

        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";
        public async Task<ResponseResult<bool>> AddCommentToMediaAsync(CreateMediaCommentDto createCommentDto)
        {
            var result = new ResponseResult<bool>();
            try
            {
                var comment = new MediaComment
                {
                    MediaId = createCommentDto.MediaId,
                    UserId = createCommentDto.UserId,
                    Comment = createCommentDto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MediaComments.Add(comment);
                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Result = true;
                result.Message = "Comment added successfully.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Result = false;
                result.Message = "An error occurred while adding the comment: " + ex.Message;
            }

            return result;
        }


        public async Task<ResponseResult<List<MediaCommentDto>>> GetCommentsByMediaIdAsync(int mediaId, int loggedInUserId)
        {
            var result = new ResponseResult<List<MediaCommentDto>>();
            try
            {
                var totalComments = await _context.MediaComments.CountAsync(c => c.MediaId == mediaId);
                var comments = await _context.MediaComments
                    .Where(c => c.MediaId == mediaId)
                    .Select(c => new MediaCommentDto
                    {
                        Id = c.Id,
                        Comment = c.Comment,
                        CreatedAt = c.CreatedAt,
                        TotalComments = totalComments,
                        User = new UserModel
                        {
                            Id = c.User.Id,
                            Username = c.User.Username,
                            ProfilePic = c.User.ProfilePic
                        },
                        IsForLoggedInUser = c.User.Id == loggedInUserId // Set based on the logged-in user's ID
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Message = "Comments fetched";
                result.Result = comments; // List of comments for the media
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "An error occurred while retrieving comments.";
                result.Result = null;
                // Log the exception
            }
            return result;
        }


        public async Task<ResponseResult<bool>> DeleteCommentToMediaAsync(DeleteCommentModel model)
        {
            var result = new ResponseResult<bool>();
            try
            {
                var comment = await _context.MediaComments.FindAsync(model.commentId);

                if (comment == null)
                {
                    result.IsSuccess = false;
                    result.Result = false;
                    result.Message = "Comment not found.";
                    return result;
                }

                _context.MediaComments.Remove(comment);
                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Result = true;
                result.Message = "Comment deleted successfully.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Result = false;
                result.Message = "An error occurred while deleting the comment: " + ex.Message;
            }

            return result;
        }

    }
}
