using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Models.InputModels
{
    public class CreateMediaCommentDto
    {
        public int MediaId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
    }


}
