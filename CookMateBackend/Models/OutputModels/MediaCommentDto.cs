using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Models.InputModels
{
    public class MediaCommentDto
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalComments { get; set; }
        public UserModel User { get; set; }
    }

}
