namespace CookMateBackend.Models.OutputModels
{
    public class UserPostsModel
    {
        public int Id { get; set; }
        public byte PostType { get; set; }
        public RecipeDto Recipe { get; set; }
        public MediaDto Media { get; set; }
    }

    public class RecipeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PreparationTime { get; set; }
        public string Media { get; set; }

    }

    public class MediaDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public byte MediaType { get; set; }
        public string MediaData { get; set; }
    }



}
