using System;

namespace NEXUSDataLayerScaffold.Models;

public class ItemsPagingParameterModel
{
    private const int maxPageSize = 20;

    public Guid guid;

    public string name { get; set; }

    public Guid seriesguid { get; set; }

    public string fields { get; set; }

    public int pageNumber { get; set; } = 1;

    public int _pageSize { get; set; } = 10;

    public int pageSize
    {
        get => _pageSize;
        set => _pageSize = value > maxPageSize ? maxPageSize : value;
    }
}