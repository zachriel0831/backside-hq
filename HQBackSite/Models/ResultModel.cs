namespace HQBackSite.Models
{
    public class ResultModel
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; } = null;
    }

    public class ResultDataModel<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }
}
