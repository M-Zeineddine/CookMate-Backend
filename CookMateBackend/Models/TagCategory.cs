using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class TagCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDuplicatable { get; set; }

    public bool IsMain { get; set; }

    public virtual ICollection<TagsList> TagsLists { get; set; } = new List<TagsList>();
}
