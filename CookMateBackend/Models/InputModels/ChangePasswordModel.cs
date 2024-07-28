namespace CookMateBackend.Models.InputModels
{
    public class ChangePasswordModel
    {
        public int UserId { get; set; } // Or use Username/Email as per your user identification strategy
        public string NewPassword { get; set; }
    }


}
