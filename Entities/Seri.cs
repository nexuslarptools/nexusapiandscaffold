using System;
using System.Collections.Generic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class Seri
{
    public Guid Guid { get; set; }
    public string Title { get; set; }
    public string Titlejpn { get; set; }
    public List<Tag> Tags { get; set; }
    public bool? Isactive { get; set; }
    public DateTime Createdate { get; set; }
    public DateTime? Deactivedate { get; set; }
}