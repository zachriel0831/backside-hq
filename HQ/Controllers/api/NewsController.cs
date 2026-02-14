using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;

namespace HQ.Controllers
{
    public class NewsController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

        /// <summary>
        /// 公告 搜尋
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage NewsRead([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql_select = null;
                string sql = "Select des_no,dept,descpt,background,urlpath,back_type,priority,type,CONVERT(char(12),start_date,111) as s,CONVERT(char(12),end_date,111) as e, dept From News " +
                             "order by type, priority ,create_date desc";
                SqlCommand command = new SqlCommand(sql, connection);
                if (dic.Any())
                {
                    foreach (var OneItem in dic)
                    {
                        if (OneItem.Value != "null")
                        {
                            sql_select += " and " + OneItem.Key + "=@" + OneItem.Key;
                            command.Parameters.Add(new SqlParameter() { ParameterName = OneItem.Key, Value = OneItem.Value });
                        }
                    }
                    command.CommandText = "Select des_no,descpt,urlpath,background,back_type,priority,type,CONVERT(char(12),start_date,111) as s,CONVERT(char(12),end_date,111) as e, dept From News where 1=1 "
                                            + sql_select + " order by type, priority ,create_date desc";

                }

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 公告 新增
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int NewsCreate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                //20221003,back_type,此為點閱率欄位,寫入0,不然後續無法累加
                string sql = "insert into News (dept, descpt, background, urlpath, priority, type ,start_date, end_date, create_user, create_date, back_type)" +
                              "values(@dept, @descpt, @background, @urlpath, @priority, @type, @start_date, @end_date, @create_user, @create_date, 0 )";
                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@descpt", Value = dic["descpt"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@background", Value = dic["background"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@urlpath", Value = dic["urlpath"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@priority", Value = dic["priority"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@type", Value = dic["type"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@start_date", Value = dic["start_date"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@end_date", Value = dic["end_date"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@create_user", Value = ((FormsIdentity)User.Identity).Ticket.Name });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@create_date", Value = DateTime.UtcNow });

                try
                {
                    iCount = command.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    string a = e.ToString();
                }
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 公告 更新
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int NewsUpdate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "UPDATE News Set descpt= @descpt ,urlpath= @urlpath  ,background= @background  ,priority= @priority, " +
                             " type = @type , start_date = @start_date ,end_date = @end_date " +
                             " , update_date=getdate() Where des_no =@des_no ";

                SqlCommand command = new SqlCommand(sql, connection);

                DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                dtFormat.ShortDatePattern = "yyyy/MM/dd";

                command.Parameters.Add(new SqlParameter() { ParameterName = "@descpt", Value = dic["descpt"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@background", Value = dic["background"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@urlpath", Value = dic["urlpath"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@priority", Value = dic["priority"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@type", Value = dic["type"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@start_date", Value = Convert.ToDateTime(dic["start_date"], dtFormat) });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@end_date", Value = dic["end_date"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@update_date", Value = DateTime.UtcNow.ToString() });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@des_no", Value = dic["des_no"] });

                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 公告 刪除
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int NewsDelete([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Delete from News where des_no = @des_no ", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@des_no", Value = dic["des_no"] });

                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }
    }
}
