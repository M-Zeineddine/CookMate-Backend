namespace CookMateBackend.Models.OutputModels
{
    public class FollowerInfo
    {
        public int FollowerId { get; set; }
        public string FollowerName { get; set; } // Adjust as per your user model
        public bool IsFollowingBack { get; set; }
    }

}
