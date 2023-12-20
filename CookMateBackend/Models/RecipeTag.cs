using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class RecipeTag
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int TagListId { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual TagsList TagList { get; set; } = null!;
}
