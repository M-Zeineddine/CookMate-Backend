using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class TagsList
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int TagCategoryId { get; set; }

    public virtual ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();

    public virtual TagCategory TagCategory { get; set; } = null!;

    public virtual ICollection<UserPreferencesTag> UserPreferencesTags { get; set; } = new List<UserPreferencesTag>();
}
