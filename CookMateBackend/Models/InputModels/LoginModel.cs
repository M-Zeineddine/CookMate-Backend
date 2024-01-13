namespace CookMateBackend.Models.InputModels
{
    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }  // For simplicity. In a real application, you should never send or store plain passwords.
    }

}
