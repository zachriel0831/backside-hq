using System.Collections.Generic;

namespace HQBackSite.Models
{
    public class IcsGroupModel
    {
        public List<IcsListModel> list { get; set; }

        public List<IcsUploadModel> uploads { get; set; }
    }
}