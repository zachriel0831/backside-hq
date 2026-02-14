namespace HQBackSite.Models
{
    public class ParaModel : PageModel
    {
        public string type_name { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string code_name { get; set; } = string.Empty;
        public string data1 { get; set; } = string.Empty;
        public string data2 { get; set; } = string.Empty;
        public string data3 { get; set; } = string.Empty;
        public string data4 { get; set; } = string.Empty;
        public string data5 { get; set; } = string.Empty;
        public string data6 { get; set; } = string.Empty;

        #region Ext

        public string data6text
        {
            get
            {
                switch (data6)
                {
                    case "black":
                        return "黑色";
                    case "blue":
                        return "藍色";
                    case "green":
                        return "綠色";
                    case "red":
                        return "紅色";
                    case "skin":
                        return "膚色";
                    case "violet":
                        return "紫色";

                    default:
                        return data6; // 找不到就回傳原值，避免空白
                }
            }
        }

        #endregion
    }
}