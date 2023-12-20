namespace CookMateBackend.Models.OutputModels
{
    public class UserDetailsModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public byte Role { get; set; }
        public string ProfilePic { get; set; }
        public string? Bio { get; set; }
        public int UserId { get; set; } // User's ID
        public int FollowersCount { get; set; } // Number of followers
        public int FollowingsCount { get; set; } // Number of followings
        public int RecipesCount { get; set; } // Number of recipes created by the user
    }
}
