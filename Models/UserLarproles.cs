using System;

namespace NEXUSDataLayerScaffold.Models;

public class UserLarproles
{
    public int Id { get; set; }
    public Guid? Userguid { get; set; }
    public Guid? Larpguid { get; set; }
    public int? Roleid { get; set; }
    public bool? Isactive { get; set; }

    public virtual Larps Larpgu { get; set; }
    public virtual Roles Role { get; set; }
    public virtual Users Usergu { get; set; }
}