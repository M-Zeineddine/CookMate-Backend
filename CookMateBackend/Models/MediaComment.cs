namespace CookMateBackend.Models
{
    public class MediaComment
    {
        public int Id { get; set; }
        public int MediaId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Media Media { get; set; }
        public User User { get; set; }
    }

}
