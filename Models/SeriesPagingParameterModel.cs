using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Models
{
    public class SeriesPagingParameterModel
    {
        const int maxPageSize = 20;

        public Guid guid;

        public string titleinput { get; set; }

        public string jpntitleinput { get; set; }

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
