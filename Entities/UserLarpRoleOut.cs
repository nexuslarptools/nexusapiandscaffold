using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NEXUSDataLayerScaffold.Entities;

public class UserLarpRoleOut
{
    public UserLarpRoleOut()
    {
        Roles = new List<RoleOut>();
    }

    public Guid? LarpGuid { get; set; }
    public string LarpName { get; set; }
    public List<RoleOut> Roles { get; set; }
}

public class RoleOut
{
    public RoleOut(int iD, string roleName)
    {
        RoleID = iD;
        RoleName = roleName;
    }

    public int RoleID { get; set; }
    public string RoleName { get; set; }
}

public class RoleIDFirst : Comparer<RoleOut>
{
    public override int Compare([AllowNull] RoleOut x, [AllowNull] RoleOut y)
    {
        if (x.RoleID.CompareTo(y.RoleID) != 0)
            return x.RoleID.CompareTo(y.RoleID);
        return 0;
    }
}