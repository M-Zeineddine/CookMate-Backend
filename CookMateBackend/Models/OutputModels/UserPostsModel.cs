namespace CookMateBackend.Models.OutputModels
{
    public class UserPostsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Media_id { get; set; }
        public int PreparationTime { get; set; }
        public byte PostType { get; set; }
    }

}
