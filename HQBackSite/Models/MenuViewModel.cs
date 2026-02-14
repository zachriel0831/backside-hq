using System.Collections.Generic;

namespace HQBackSite.Models
{
    public class MenuViewModel
    {
        public List<MenuGroupModel> MenuGroups { get; set; }
        public string CurrentModule { get; set; }
        
        public MenuViewModel()
        {
            MenuGroups = new List<MenuGroupModel>();
        }
    }
}
