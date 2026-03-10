using System;

namespace HQBackSite.Models
{
    /// <summary>
    /// 菜單資源 Model
    /// </summary>
    public class MenuResourceModel
    {
        public long Id { get; set; }
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public long? ParentId { get; set; }
        public string RoutePath { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string Icon { get; set; }
        public int DisplayOrder { get; set; }
        public byte Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 用戶菜單權限 Model
    /// </summary>
    public class UserMenuPermissionModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long MenuId { get; set; }
        public DateTime GrantedAt { get; set; }
        public string GrantedBy { get; set; }
    }

    /// <summary>
    /// 用戶權限查詢回應 Model
    /// </summary>
    public class UserMenuPermissionsResponse
    {
        public string UserId { get; set; }
        public string[] MenuCodes { get; set; }
    }
}
