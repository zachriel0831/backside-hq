using System.Collections.Generic;

namespace HQBackSite.Models
{
    public class PagedListModel<T> : List<T>, IPagedList
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNo > 1;
        public bool HasNextPage => PageNo < TotalPages;

        public PagedListModel(List<T> items, int pageNo, int pageSize, int totalCount)
        {
            this.AddRange(items);
            PageNo = pageNo;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
