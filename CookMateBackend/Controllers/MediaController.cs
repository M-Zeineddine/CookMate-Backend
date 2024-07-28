using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Mvc;

namespace CookMateBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaRepository _repository;

        public MediaController(IMediaRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        [Route("addComment")]
        public async Task<ResponseResult<bool>> AddComment([FromBody] CreateMediaCommentDto commentDto)
        {
            return await _repository.AddCommentToMediaAsync(commentDto);
        }

        [HttpGet]
        [Route("getComments")]
        public async Task<ResponseResult<List<MediaCommentDto>>> GetComments(int mediaId, int userId)
        {
            return await _repository.GetCommentsByMediaIdAsync(mediaId, userId);
        }

        [HttpPost]
        [Route("deletComment")]
        public async Task<ResponseResult<bool>> DeletComment(DeleteCommentModel model)
        {
            return await _repository.DeleteCommentToMediaAsync(model);
        }
    }
}
