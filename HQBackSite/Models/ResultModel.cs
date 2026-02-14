namespace HQBackSite.Models
{
    public class ResultModel
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; } = null;
    }

    public class ResultDataModel<T> : ResultModel
    {
        public new T Data { get; set; }
    }
}