using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class SeriInput
{
    public Guid Guid { get; set; }
    public string Title { get; set; }
    public string Titlejpn { get; set; }
    public List<Guid> Tags { get; set; }
    public bool? Isactive { get; set; }
    public DateTime Createdate { get; set; }
    public DateTime? Deactivedate { get; set; }
}