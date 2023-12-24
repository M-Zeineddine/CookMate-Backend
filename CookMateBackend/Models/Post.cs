using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Post
{
    public int Id { get; set; }

    public byte Type { get; set; }

    public int UserId { get; set; }

    public int? RecipeId { get; set; }

    public int? MediaId { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Media? Media { get; set; }

    public virtual Recipe? Recipe { get; set; }

    public virtual User User { get; set; } = null!;
}
