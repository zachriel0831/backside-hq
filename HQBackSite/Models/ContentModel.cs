using System;

namespace HQBackSite.Models
{
    public class ContentModel : PageModel
    {
        public string page { get; set; }
        public string dept { get; set; }
        public string subtype { get; set; }
        public int subject_id { get; set; }
        public int priority { get; set; }
        public string subject { get; set; }
        public string url { get; set; }
        public string content { get; set; }
        public string is_show { get; set; }
        public DateTime? create_date { get; set; }

        #region Ext

        public string csubject { get; set; }
        public string data1 { get; set; }
        public string style { get; set; }
        public string subtypetext
        {
            get
            {
                switch (subtype)
                {
                    //case "d_banner":
                    //    return "橫幅 Banner";
                    case "d_dw":
                        return "下載區塊";
                    //case "d_left":
                    //    return "左側區塊";
                    //case "d_left1":
                    //    return "左側區塊一";
                    case "d_main":
                        return "主內容區塊";
                    case "d_main1":
                        return "主內容區塊一";
                    case "d_main2":
                        return "主內容區塊二";
                    case "d_main3":
                        return "主內容區塊三";
                    case "d_main4":
                        return "主內容區塊四";
                    case "d_main5":
                        return "主內容區塊五";

                    case "d_right":
                        return "右側區塊";
                    case "d_right1":
                        return "右側區塊一";
                    case "d_right2":
                        return "右側區塊二";
                    case "d_right3":
                        return "右側區塊三";
                    case "d_right4":
                        return "右側區塊四";

                    //case "d_top":
                    //    return "上方區塊";

                    default:
                        return subtype; // 找不到就回傳原值，避免空白
                }
            }
        }

        #endregion
    }
}