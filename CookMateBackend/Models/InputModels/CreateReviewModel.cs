namespace CookMateBackend.Models.InputModels
{
    public class CreateReviewModel
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
    }
}
