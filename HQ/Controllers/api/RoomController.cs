using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Http;

using System.Net;
using System.Configuration;

namespace HQ.Controllers
{
    public class RoomController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;


        [HttpPost]
        public HttpResponseMessage Read([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT type_name,code_name , data1 , data2 FROM para where type = '1050'  ", connection);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        [HttpPost]
        public int Create([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO otb_edu_userinfo (user_id, phone, address) VALUES (@user_id, @phone, @address)", connection);

                command.Parameters.Add(new SqlParameter() { ParameterName = "@user_id", Value = dic["user_id"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@phone", Value = dic["phone"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@address", Value = dic["address"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        [HttpPost]
        public int Update([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("UPDATE otb_edu_userinfo SET phone = @phone ,address = @address WHERE user_id = @user_id", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@user_id", Value = dic["user_id"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@phone", Value = dic["phone"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@address", Value = dic["address"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }

        [HttpPost]
        public int Deleted([FromBody]Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("DELETE FROM otb_edu_userinfo WHERE user_id = @user_id ", connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@user_id", Value = dic["user_id"] });
                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }


    }
}
