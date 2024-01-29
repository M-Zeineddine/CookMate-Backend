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


        public async Task<ResponseResult<List<MediaCommentDto>>> GetCommentsByMediaIdAsync(int mediaId)
        {
            var result = new ResponseResult<List<MediaCommentDto>>();
            try
            {
                var comments = await _context.MediaComments
                    .Where(c => c.MediaId == mediaId)
                    .Select(c => new MediaCommentDto
                    {
                        Id = c.Id,
                        Comment = c.Comment,
                        CreatedAt = c.CreatedAt,
                        User = new UserModel
                        {
                            Id = c.User.Id,
                            Username = c.User.Username,
                            ProfilePic = c.User.ProfilePic
                        }
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
                // Again, in a real-world scenario, you might want to log this exception
            }
            return result;
        }
    }
}
