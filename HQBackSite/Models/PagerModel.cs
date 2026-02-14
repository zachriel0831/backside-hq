namespace HQBackSite.Models
{
    public class PagerModel
    {
        public string ActionName { get; set; }
        public IPagedList PagedList { get; set; }
        public object QueryModel { get; set; }

        public PagerModel(string actionName, IPagedList pagedList, object queryModel)
        {
            ActionName = actionName;
            PagedList = pagedList;
            QueryModel = queryModel;
        }
    }

    public interface IPagedList
    {
        int PageNo { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
