namespace CookMateBackend.Models.InputModels
{
    public class UserFollowModel
    {
        public int LoggedInUserId { get; set; }
        public int TargetUserId { get; set; }
    }

}
