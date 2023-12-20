﻿using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Medium
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string Descirption { get; set; } = null!;

    public byte MediaType { get; set; }

    public string MediaData { get; set; } = null!;

    public int? RecipeId { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Recipe? Recipe { get; set; }
}
