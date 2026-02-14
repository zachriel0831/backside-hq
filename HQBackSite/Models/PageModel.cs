namespace HQBackSite.Models
{
    public class PageModel
    {
        public int PageNo
        {
            get
            {
                return _pageNo;
            }

            set
            {
                _pageNo = value > 0 ? value : 1;
            }
        }
        private int _pageNo = 1;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }

            set
            {
                _pageSize = value > 0 ? value : 10;
            }
        }
        private int _pageSize = 10;

        public int TotalCount { get; set; }
    }
}