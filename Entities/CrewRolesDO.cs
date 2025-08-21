using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace NEXUSDataLayerScaffold.Entities
{
    public class CrewRolesDO
    {
        public List<CrewRole> Roles { get; set; }

        public CrewRolesDO()
        {
            Roles = new List<CrewRole>();
        }
    }
}

public class CrewRole
{
    public Guid Guid { get; set; }
    public string Position { get; set; }
    public string Description { get; set; }
    public int ord {get; set;}

    public CrewRole()
    {

    }

    public CrewRole(ShipCrewList scl)
    {
        Guid = scl.Guid;
        ord = scl.Ord;
        Position = scl.Position;
        Description = scl.Details;
    }

    public void Default()
    {
        Guid = Guid.Empty;
        ord = 10000;
        Position = "Custom";
        Description = string.Empty;
    }

    public ShipCrewList ToShipCrewList()
    {
        return new ShipCrewList()
        {
            Guid = Guid,
            Ord = ord,
            Position = Position,
            Details = Description
        };
    }

}

