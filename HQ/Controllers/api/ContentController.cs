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
    public class ContentController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

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
                SqlCommand command = new SqlCommand("select distinct dept from content", connection);
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
                SqlCommand command = new SqlCommand("select distinct page from content", connection);
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
                SqlCommand command = new SqlCommand("select distinct subtype from content", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 單元內容維護搜尋
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage ContentRead([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql_select = null;
                string sql = "select Dept, Page, subtype, subject_id, subject, url, content, is_show from content where 1=1 order by dept, page, subtype";
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
                    command.CommandText = "select Dept, Page, subtype, subject_id, subject, url, content, is_show from content where 1=1" + sql_select + " order by dept, page, subtype";

                }

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 單元內容維護 新增
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int ContentCreate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "INSERT INTO content(Dept, Page, subtype, subject, url, content, is_show) values(@dept , @page , @subtype, @subject , @url , @content, @is_show )";
                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subtype", Value = dic["subtype"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subject", Value = dic["subject"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@url", Value = dic["url"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@content", Value = dic["content"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@is_show", Value = dic["is_show"] });

                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 單元內容維護 更新
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int ContentUpdate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "update content set subject= @subject , url= @url , content= @content, is_show= @is_show " +
                             " where page= @page and dept= @dept and subtype= @subtype and subject_id= @subject_id";

                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subtype", Value = dic["subtype"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subject", Value = dic["subject"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@url", Value = dic["url"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@content", Value = dic["content"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@is_show", Value = dic["is_show"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subject_id", Value = dic["subject_id"] });

                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 單元內容維護 刪除
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int ContentDelete([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("delete from content where page= @page and dept= @dept and subtype= @subtype and subject_id= @subject_id", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subtype", Value = dic["subtype"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@subject_id", Value = dic["subject_id"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }
    }
}
