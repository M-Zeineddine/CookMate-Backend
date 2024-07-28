namespace CookMateBackend.Models.OutputModels
{
    public class UserReviewModel
    {
        public int Id { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserModel User { get; set; }
        public bool IsForLoggedInUser { get; set; } // New property
    }
}
