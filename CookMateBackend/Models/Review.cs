using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Review
{
    public int Id { get; set; }

    public decimal Rating { get; set; }

    public string? Comment { get; set; }

    public int UserId { get; set; }

    public int RecipesId { get; set; }

    public virtual Recipe? Recipes { get; set; }

    public virtual User? User { get; set; }
}
