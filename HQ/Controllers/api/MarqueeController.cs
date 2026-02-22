using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HQ.Controllers
{
    public class MarqueeController : ApiController
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

        /// <summary>
        /// 跑馬燈設定（News.type = light）
        /// </summary>
        [HttpPost]
        public HttpResponseMessage Read([FromBody] Dictionary<string, string> dic)
        {
            var ds = new DataSet();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var sql = @"
SELECT *
FROM News WITH(NOLOCK)
WHERE type = 'light'
  AND dept = N'跑馬燈設定'
  AND CONVERT(varchar(8), GETDATE(), 112) BETWEEN CONVERT(varchar(8), start_date, 112) AND CONVERT(varchar(8), end_date, 112)
ORDER BY ISNULL(priority, 999999), create_date DESC";

                var command = new SqlCommand(sql, connection);
                var sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
    }
}
