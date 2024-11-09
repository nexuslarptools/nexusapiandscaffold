using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Role
{
    public int Id { get; set; }

    public string Rolename { get; set; }

    public int? Ord { get; set; }

    public virtual ICollection<UserLarprole> UserLarproles { get; } = new List<UserLarprole>();
}