using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Follower
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int FollowerId { get; set; }

    public virtual User FollowerNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
