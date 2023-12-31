using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class LarprunPreReg
{
    public Guid Guid { get; set; }

    public Guid LarprunGuid { get; set; }

    public Guid UserGuid { get; set; }

    public Guid? CharactersheetChoice1 { get; set; }

    public Guid? CharactersheetChoice2 { get; set; }

    public Guid? CharactersheetChoice3 { get; set; }

    public string CharactersheetCustomchoice1 { get; set; }

    public string CharactersheetCustomchoice1Series { get; set; }

    public string CharactersheetCustomchoice2 { get; set; }

    public string CharactersheetCustomchoice2Series { get; set; }

    public string CharactersheetCustomchoice3 { get; set; }

    public string CharactersheetCustomchoice3Series { get; set; }

    public Guid? CharactersheetRegistered { get; set; }

    public bool? CharactersheetRegisteredApprovedsheet { get; set; }

    public Guid? CharactersheetRegisteredApprovedbyUser { get; set; }

    public bool? Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public virtual User CharactersheetRegisteredApprovedbyUserNavigation { get; set; }

    public virtual Larprun Larprun { get; set; }

    public virtual User User { get; set; }
}
