using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class UserPreferencesTag
{
    public int PreferenceTagId { get; set; }

    public int? UserId { get; set; }

    public int? TagId { get; set; }

    public virtual TagsList? Tag { get; set; }

    public virtual User? User { get; set; }
}
