using System;

namespace NEXUSDataLayerScaffold.Models;

public class SeriesTag
{
    public int Id { get; set; }

    public Guid SeriesGuid { get; set; }

    public Guid TagGuid { get; set; }

    public virtual Series Series { get; set; }

    public virtual Tag Tag { get; set; }
}