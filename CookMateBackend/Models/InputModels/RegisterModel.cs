namespace CookMateBackend.Models.InputModels
{
    public class RegisterModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        // Add other fields as necessary, e.g. Role, ProfilePic, Bio...
        public IFormFile? ProfilePic { get; set; }
        public string? Bio { get; set; }
    }
}
