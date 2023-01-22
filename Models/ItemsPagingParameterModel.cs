using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace NEXUSDataLayerScaffold.Models
{
    public class ItemsPagingParameterModel
    {
        const int maxPageSize = 20;

        public Guid guid;

        public string name { get; set; }

        public Guid seriesguid { get; set; }

        public string fields { get; set; }
        public bool? userCreated { get; set; }
        public bool? userApproved { get; set; }

        public int pageNumber { get; set; } = 1;

        public int _pageSize { get; set; } = 10;

        public int pageSize
        {
            get { return _pageSize; }
            set
            { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }
    }
}
