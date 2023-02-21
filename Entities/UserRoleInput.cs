using System;

namespace NEXUSDataLayerScaffold.Entities;

public class UserRoleInput
{
    public Guid Guid { get; set; }

    public string RoleName { get; set; }

    public Guid? LarpGuid { get; set; }
}