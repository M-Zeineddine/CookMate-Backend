namespace CookMateBackend.Models.OutputModels
{
    public class UserMediasModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MediaData { get; set; }
        public byte MediaType { get; set; }
        public byte PostType { get; set; }
    }

}
