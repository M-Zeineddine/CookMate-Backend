using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Media
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? MediaData { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool is_deleted { get; set; }
    public int Likes { get; set; }

    public int? RecipeId { get; set; }
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual Recipe? Recipe { get; set; }
    public virtual ICollection<MediaComment> MediaComments { get; set; }

}
