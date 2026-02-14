using System.Collections.Generic;

namespace HQBackSite.Models
{
    public class MenuGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<MenuItemModel> Items { get; set; }
        
        public MenuGroupModel()
        {
            Items = new List<MenuItemModel>();
        }
    }
}
