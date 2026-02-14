namespace HQBackSite.Models
{
    public class UserModel
    {
        public string ORGAN_ID { get; set; }
        public string DEPARTMENT { get; set; }
        public string EMPLOYEE_ID { get; set; }
        public string LOCAL_NAME { get; set; }
        public string EMAIL { get; set; }

        #region Ext

        public string Account { get; set; }

        public string Password { get; set; }

        #endregion
    }
}