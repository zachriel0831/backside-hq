using System;

namespace HQBackSite.Models
{
    public class ParaPermissionModel
    {
        public int id { get; set; }
        public string emid { get; set; }
        public string code_name { get; set; }
        public DateTime create_date { get; set; }
        public string create_user { get; set; }
    }
}