using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HQ.Controllers
{
    public class IcsController : ApiController
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

        /// <summary>
        /// 國際認證規範資料查詢
        /// 來源：news (type='ics') LEFT JOIN ics_list
        /// </summary>
        /// <param name="dic">可選條件：des_no, dept, background</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage IcsRead([FromBody] Dictionary<string, string> dic)
        {
            var ds = new DataSet();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var sql = @"
SELECT
    n.*,
    l.*
FROM News n WITH(NOLOCK)
LEFT JOIN ics_list l WITH(NOLOCK)
    ON l.desno = n.des_no
    AND ISNULL(l.deleted, 0) = 0
WHERE n.type = 'ics'
  AND n.dept IN (N'ISO文件', N'上傳文件', N'資安文件')
  AND (@des_no IS NULL OR @des_no = '' OR CAST(n.des_no AS VARCHAR(20)) = @des_no)
  AND (@dept IS NULL OR @dept = '' OR n.dept = @dept)
  AND (@background IS NULL OR @background = '' OR n.background LIKE '%' + @background + '%')
ORDER BY ISNULL(n.priority, 999999), n.create_date DESC, l.listID ASC";

                var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@des_no", Value = (object)GetValue(dic, "des_no") ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = (object)GetValue(dic, "dept") ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@background", Value = (object)GetValue(dic, "background") ?? DBNull.Value });

                var sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);

                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        private static string GetValue(Dictionary<string, string> dic, string key)
        {
            if (dic == null || string.IsNullOrWhiteSpace(key) || !dic.ContainsKey(key))
            {
                return null;
            }

            var value = dic[key];
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return value;
        }
    }
}
