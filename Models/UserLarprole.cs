using System;

namespace NEXUSDataLayerScaffold.Models;

public class UserLarprole
{
    public int Id { get; set; }

    public Guid? Userguid { get; set; }

    public Guid? Larpguid { get; set; }

    public int? Roleid { get; set; }

    public bool? Isactive { get; set; }

    public virtual Larp Larp { get; set; }

    public virtual Role Role { get; set; }

    public virtual User User { get; set; }
}