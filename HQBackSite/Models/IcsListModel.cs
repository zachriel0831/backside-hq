using System;

namespace HQBackSite.Models
{
    public class IcsListModel
    {
        public int ListID { get; set; }
        public int des_no { get; set; }
        public string Category { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public string DocName { get; set; } = string.Empty;
        public string DocUrl { get; set; } = string.Empty;
        public string AttachmentInfo { get; set; } = string.Empty;
        public string MainUser { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public int Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; } = string.Empty;
    }
}