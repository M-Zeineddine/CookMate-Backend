using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IMediaRepository
    {
        Task<ResponseResult<bool>> AddCommentToMediaAsync(CreateMediaCommentDto createCommentDto);
        Task<ResponseResult<List<MediaCommentDto>>> GetCommentsByMediaIdAsync(int mediaId);
    }

}
