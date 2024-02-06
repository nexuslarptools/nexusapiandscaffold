using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Roles
{
    public Roles()
    {
        UserLarproles = new HashSet<UserLarproles>();
    }

    public int Id { get; set; }
    public string Rolename { get; set; }
    public int Ord { get; set; }

    public virtual ICollection<UserLarproles> UserLarproles { get; set; }
}