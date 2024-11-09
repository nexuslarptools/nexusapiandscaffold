using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class ListSeries
{
    public ListSeries()
    {
        Series = new List<ShortSeriesInfo>();
    }

    public List<ShortSeriesInfo> Series { get; set; }

    public void Add(Series iSeries)
    {
        var newinfo = new ShortSeriesInfo();
        newinfo.Guid = iSeries.Guid;
        newinfo.Name = iSeries.Title;
        Series.Add(newinfo);
    }
}

public class ShortSeriesInfo
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
}