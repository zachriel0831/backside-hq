using System.Collections.Generic;

namespace HQBackSite.Models
{
    public class MenuItemModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string ModuleId { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public bool IsActive { get; set; }
    }
}
