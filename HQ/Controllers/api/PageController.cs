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
    public class PageController : ApiController
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
                SqlCommand command = new SqlCommand("select distinct dept from page", connection);
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
                SqlCommand command = new SqlCommand("select distinct page from page", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 網頁維護 搜尋
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage PageRead([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql_select = null;
                string sql = "select page, dept, title, url, script, is_show from page where 1=1 order by dept, page";
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
                    command.CommandText = "select page, dept, title, url, script, is_show from page  where 1=1 " + sql_select + "  order by dept, page";

                }

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 網頁維護 新增
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int PageCreate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO page(page, dept, title, url, is_show) VALUES (@page, @dept, @title, @url, @is_show)", connection);

                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@title", Value = dic["title"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@url", Value = dic["url"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@is_show", Value = dic["is_show"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 網頁維護 更新
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int PageUpdate([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("UPDATE page set title = @title, url = @url, is_show = @is_show  where page = @page and dept = @dept", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@title", Value = dic["title"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@url", Value = dic["url"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@is_show", Value = dic["is_show"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        /// <summary>
        /// 網頁維護 刪除
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public int PageDelete([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("DELETE FROM page where page = @page and dept = @dept", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

      
    }
}
