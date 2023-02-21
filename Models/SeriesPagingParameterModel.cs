using System;

namespace NEXUSDataLayerScaffold.Models;

public class SeriesPagingParameterModel
{
    private const int maxPageSize = 20;

    public Guid guid;

    public Guid[] tags { get; set; }

    public string titleinput { get; set; }

    public string jpntitleinput { get; set; }

    public int pageNumber { get; set; } = 1;

    public int _pageSize { get; set; } = 10;

    public int pageSize
    {
        get => _pageSize;
        set => _pageSize = value > maxPageSize ? maxPageSize : value;
    }
}