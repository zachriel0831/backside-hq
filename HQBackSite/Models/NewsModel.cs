using System;
using System.Collections.Generic;

namespace HQBackSite.Models
{
    public class NewsModel : PageModel
    {
        public int des_no { get; set; }
        public string dept { get; set; }
        public string descpt { get; set; }
        public string background { get; set; }
        public int priority { get; set; }
        public string type { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string create_user { get; set; }
        public DateTime create_date { get; set; }
        public DateTime? update_date { get; set; }

        #region Ext

        public string status { get; set; }

        public string urlpath { get; set; }

        public IcsGroupModel IcsGroup { get; set; } 

        // 前端使用 JSON POST 時，ICS 明細會放在 request body 中
        public string icsSecurityData { get; set; }
        public string icsIsoData { get; set; }

        #endregion
    }
}