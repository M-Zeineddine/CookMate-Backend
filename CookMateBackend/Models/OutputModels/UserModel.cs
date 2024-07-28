namespace CookMateBackend.Models.OutputModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? ProfilePic { get; set; }
        public string? Email { get; set; }
        public bool IsFollowing { get; set; }
    }
}
