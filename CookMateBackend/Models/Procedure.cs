using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Procedure
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Media { get; set; } = null!;

    public byte MediaType { get; set; }

    public int Time { get; set; }

    public byte Step { get; set; }

    public int RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
}
