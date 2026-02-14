using System;

namespace HQBackSite.Models
{
    public class IcsUploadModel
    {
        public int UpdateID { get; set; }
        public int des_no { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileContentType { get; set; } = string.Empty;
        public int Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; } = string.Empty;
    }
}