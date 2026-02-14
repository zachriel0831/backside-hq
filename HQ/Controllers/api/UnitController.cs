using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HQ.Controllers
{
    public class UnitController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

        /// <summary>
        /// 網頁維護 顏色下拉式選單  
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage StyleSearch([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select code_name from para where type='1010'", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 網頁維護 部門下拉式選單  
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeptSearch([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select distinct dept from unit", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
             /// <summary>
          /// 網頁維護 頁數下拉式選單  
          /// </summary>
          /// <param name="dic"></param>
          /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage PageSearch([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select distinct page from unit", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 網頁維護 單元下拉式選單  
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SubtypeSearch([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select distinct subtype from unit", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 單元維護 搜尋
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UnitRead([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql_select = null;
                string sql = "select Dept, Page, subtype, unit_title, style, include_file, unit_height, unit_weight, title_pic," +
                             " vl_line, vr_line, h_line, left_up_line, right_up_line, left_down_line, right_down_line, bg_color," +
                             " subject_len, priority, is_show from unit  where 1=1 order by dept, page, subtype";
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
                    command.CommandText = "select Dept, Page, subtype, unit_title, style, include_file, unit_height, unit_weight, title_pic," +
                             " vl_line, vr_line, h_line, left_up_line, right_up_line, left_down_line, right_down_line, bg_color," +
                             " subject_len, priority, is_show from unit  where 1=1 " + sql_select + "  order by dept, page, subtype";

                }

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 單元維護 新增
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int UnitCreate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "INSERT INTO unit(Dept, Page, subtype, unit_title, style, include_file, unit_height, unit_weight, title_pic," +
                                " vl_line, vr_line, h_line, left_up_line, right_up_line, left_down_line, right_down_line, bg_color," +
                                " subject_len, is_show) values( @dept , @page , @subtype , @unit_title , @style , @include_file , 0 , 0, " +
                                " 'u_title.gif', 'u_vl.gif', 'u_vr.gif', 'u_h.gif', 'u_lu.gif', 'u_ru.gif', 'u_ld.gif', 'u_rd.gif', null " +
                                " , 0 , @is_show)";
                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subtype", Value = dic["subtype"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@unit_title", Value = dic["unit_title"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@style", Value = dic["style"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@include_file", Value = dic["include_file"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@is_show", Value = dic["is_show"] });

                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 單元維護 更新
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int UnitUpdate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string sql = "update unit set unit_title = @unit_title , style = @style, include_file = @include_file ," +
                             " is_show= @is_show where page= @page and dept= @dept and subtype= @subtype  ";
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subtype", Value = dic["subtype"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@unit_title", Value = dic["unit_title"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@style", Value = dic["style"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@include_file", Value = dic["include_file"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@is_show", Value = dic["is_show"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 單元維護 刪除
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int UnitDelete([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("delete from unit where page= @page and dept= @dept and subtype= @subtype ", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subtype", Value = dic["subtype"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }
    }
}
