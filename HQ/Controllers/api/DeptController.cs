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
    public class DeptController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
        private string NewsLetterConnectionString = ConfigurationManager.ConnectionStrings["NewsLetter"].ConnectionString;

        public DataSet TabsPage(string id)
        {
            DataSet ds = new DataSet();
            string sql = "select u.dept, u.style, u.title_pic, u.vl_line, u.vr_line, u.subtype,c.page, c.subject_id, c.subject, c.content, isnull(c.url, '') url " +
                            "from UNIT as u join content as c on u.subtype = c.subtype and u.dept = c.dept " +
                            "where u.subtype = 'd_topbtn' and u.dept = @dept and u.is_show = 'Y' and c.is_show = 'Y' order by c.page";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = id });
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return ds;
        }


        public DataSet PageContent([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            string sql = "select *,isnull(include_file,'') as strif from unit as u join content as c on u.page=c.page and u.dept=c.dept and u.subtype=c.subtype " +
                         "where u.dept = @dept and u.is_show='Y' and c.is_show='Y' order by u.page, u.subtype, c.priority";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return ds;
        }

        #region private functions
        /// <summary>
        /// to get campaign_id homepage startdate with skmit_campaign
        /// </summary>
        /// <param name="dic">department</param>
        /// <returns></returns>
        private DataSet Campaign([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "select distinct campaign_id,homepage,startdate from [001AAPW19].webmax6.webmax6." + dic["dept"] + "_campaign where campaign_id like @letter order by startdate desc";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@letter", Value = dic["letter"] + "%" });
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return ds;
        }

        /// <summary>
        /// 新版DM邏輯，需將舊英文代號轉成分店代號
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private List<string> StoreEngNameToShopStoreId(Dictionary<string, string> dic)
        {
            var storeIds = new List<string>();
            if (dic != null && dic.ContainsKey("dept"))
            {
                string storeId = dic["dept"];

                if (storeId.Contains("nansi"))
                {
                    storeIds.Add("101");
                }
                else if (storeId.Contains("tystation"))
                {
                    storeIds.Add("171");
                }
                else if (storeId.Contains("station"))
                {
                    storeIds.Add("111");
                }
                else if (storeId.Contains("sinyi"))
                {
                    storeIds.Add("121");
                    storeIds.Add("122");
                    storeIds.Add("123");
                    storeIds.Add("124");
                }
                else if (storeId.Contains("dt"))
                {
                    //Weily,20230727,新增忠孝店
                    storeIds.Add("131");
                }
                else if (storeId.Contains("tienmu"))
                {
                    storeIds.Add("151");
                }
                else if (storeId.Contains("taoyuan"))
                {
                    storeIds.Add("161");
                }
                else if (storeId.Contains("taichung"))
                {
                    storeIds.Add("201");
                }
                else if (storeId.Contains("kaohsiung"))
                {
                    storeIds.Add("301");
                }
                else if (storeId.Contains("zhongshan"))
                {
                    storeIds.Add("311");
                }
                else if (storeId.Contains("simen"))
                {
                    storeIds.Add("321");
                }
                else if (storeId.Contains("chiayi"))
                {
                    storeIds.Add("331");
                }
                else if (storeId.Contains("zuoying"))
                {
                    storeIds.Add("351");
                }
                else if (storeId.Contains("xiaobeimen"))
                {
                    storeIds.Add("361"); //小北門
                }
            }

            return storeIds;
        }

        /// <summary>
        /// 新版DM邏輯，需將專案類型轉成名稱
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private string GetNewVersionCampaignCategory(Dictionary<string, string> dic)
        {
            string categoryName = string.Empty;
            if (dic != null && dic.ContainsKey("letter"))
            {
                string ov = dic["letter"];
                if (!string.IsNullOrEmpty(ov))
                {
                    switch (ov.ToLower())
                    {
                        case "article":
                            categoryName = "函";
                            break;
                        case "bonuspenalty":
                            categoryName = "令";
                            break;
                        case "simplenotes":
                            categoryName = "簡便行文";
                            break;
                        case "other":
                            categoryName = "其他";
                            break;
                        default:
                            break;
                    }
                }
            }
            return categoryName;
        }
        #endregion

        [HttpPost]
        public HttpResponseMessage LetterData([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            string sql;
            object path = null;
            try
            {
                if (dic != null && dic["dept"] == "sinyi")
                {
                    switch (dic["letter"].ToLower())
                    {
                        case "article":
                            sql = @"select * from(
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyi/article/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyi_article_edm as p left join Attach as a on a.edm_name = p.edm_name) a order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA4/article/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA4_article_edm as p left join Attach as a on a.edm_name = p.edm_name) b order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA8/article/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA8_article_edm as p left join Attach as a on a.edm_name = p.edm_name) c order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA9/article/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA9_article_edm as p left join Attach as a on a.edm_name = p.edm_name) d order by start_date DESC
                        ) xinyi order by start_date DESC";
                            break;
                        case "bonuspenalty":
                            sql = @"select * from(
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyi/BonusPenalty/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyi_BonusPenalty_edm as p left join Attach as a on a.edm_name = p.edm_name) a order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA4/BonusPenalty/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA4_BonusPenalty_edm as p left join Attach as a on a.edm_name = p.edm_name) b order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA8/BonusPenalty/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA8_BonusPenalty_edm as p left join Attach as a on a.edm_name = p.edm_name) c order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA9/BonusPenalty/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA9_BonusPenalty_edm as p left join Attach as a on a.edm_name = p.edm_name) d order by start_date DESC
                        ) xinyi order by start_date DESC";
                            break;
                        case "other":
                            sql = @"select * from(
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyi/Other/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyi_Other_edm as p left join Attach as a on a.edm_name = p.edm_name) a order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA4/Other/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA4_Other_edm as p left join Attach as a on a.edm_name = p.edm_name) b order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA8/Other/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA8_Other_edm as p left join Attach as a on a.edm_name = p.edm_name) c order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA9/Other/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA9_Other_edm as p left join Attach as a on a.edm_name = p.edm_name) d order by start_date DESC
                        ) xinyi order by start_date DESC";
                            break;
                        case "simplenotes":
                            sql = @"select * from(
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyi/SimpleNotes/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyi_SimpleNotes_edm as p left join Attach as a on a.edm_name = p.edm_name) a order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA4/SimpleNotes/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA4_SimpleNotes_edm as p left join Attach as a on a.edm_name = p.edm_name) b order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA8/SimpleNotes/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA8_SimpleNotes_edm as p left join Attach as a on a.edm_name = p.edm_name) c order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA9/SimpleNotes/Default.htm' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA9_SimpleNotes_edm as p left join Attach as a on a.edm_name = p.edm_name) d order by start_date DESC
                        ) xinyi order by start_date DESC";
                            break;
                        case "website":
                        default:
                            sql = @"select * from(
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyi/website/' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyi_website_edm as p left join Attach as a on a.edm_name = p.edm_name) a order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA4/website/' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA4_website_edm as p left join Attach as a on a.edm_name = p.edm_name) b order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA8/website/' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA8_website_edm as p left join Attach as a on a.edm_name = p.edm_name) c order by start_date DESC
                        union all
                        select top 10 * from
                        (select  p.edm_name , p.title , len(p.title) as t ,'data\svr2/sinyiA9/website/' as path, 
                         p.id , p.start_date , a.attach_no , a.urlpath as apath 
                        from [001AAPW19].webmax6.webmax6.sinyiA9_website_edm as p left join Attach as a on a.edm_name = p.edm_name) d order by start_date DESC
                        ) xinyi order by start_date DESC";
                            break;
                    }
                }
                else
                {
                    DataSet campaign = Campaign(dic);
                    string dept = dic["dept"] + '_' + dic["letter"] + "_edm";
                    sql = "select  p.edm_name , p.title , len(p.title) as t ,@path as path, " +
                        " p.id , p.start_date , a.attach_no , a.urlpath as apath " +
                        "from [001AAPW19].webmax6.webmax6." + dept + " as p left join Attach as a on a.edm_name = p.edm_name order by start_date DESC";
                    path = campaign.Tables[0].Rows[0].ItemArray[1];
                }

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sql, connection);

                    if (path != null)
                        command.Parameters.Add(new SqlParameter() { ParameterName = "@path", Value = path });

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string oldPath = dr["path"].ToString();
                    string id = dr["id"].ToString();
                    string newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", oldPath.Substring(0, oldPath.Length - 11), id);
                    //http://od-paper.skm.com.tw/' + val.strfilepath.substring(0, val.strfilepath.length - 11) + '/wmx_edmimage/' + val.id + '/edm_content.htm',
                    dr["path"] = newPath;
                }
            }
            catch
            {
                //當ds沒有任何table時
                if (ds.Tables.Count == 0)
                {
                    //20230802,避免舊系統沒資料,造成ds沒table,而讓後續新版edm無法填入,故填充一個ds中的table
                    DataTable edmTable = ds.Tables.Add("Table");

                    DataColumn pkEdmID =
                    edmTable.Columns.Add("edm_name", typeof(string));
                    edmTable.Columns.Add("title", typeof(string));
                    edmTable.Columns.Add("t", typeof(string));
                    edmTable.Columns.Add("path", typeof(string));
                    edmTable.Columns.Add("id", typeof(string));
                    edmTable.Columns.Add("start_date", typeof(string));
                    edmTable.Columns.Add("attach_no", typeof(string));
                    edmTable.Columns.Add("apath", typeof(string));                    

                    edmTable.PrimaryKey = new DataColumn[] { pkEdmID };
                }

            }

            //2022/6/20 對照 article = 函, simpleNotes = 簡便行文, bonusPenalty = 令, other = 其他
            //新 DM 系統邏輯，只要將 dic 進行轉換(店英文名換成代碼即可)
            try
            {
                var storeids = StoreEngNameToShopStoreId(dic);
                var categoryName = GetNewVersionCampaignCategory(dic);
                if (storeids.Count > 0 && !string.IsNullOrEmpty(categoryName))
                {
                    var dt = new DataTable();
                    using (SqlConnection connection = new SqlConnection(NewsLetterConnectionString))
                    {
                        connection.Open();
                        sql = $@"SELECT co.CAMPAIGN_NAME AS [edm_name], co.CAMPAIGN_TITLE AS [title], '' AS [strfilepath], co.CONTENT_ID AS [id],
                            ISNULL(co.CAMPAIGN_LAST_SENT_DATE, co.UPDATED_TIME) AS [start_date]
                            FROM CAMPAIGN_CATEGORY c INNER JOIN CAMPAIGN_CONTENT co ON c.CATEOGRY_ID = co.CATEOGRY_ID
                            WHERE c.STORE_ID IN ('{string.Join("','", storeids)}') AND c.CATEGORY_STATUS = 1 AND co.CAMPAIGN_STATUS > 0
                            AND c.CATEGORY_NAME = '{categoryName}' ORDER BY co.CAMPAIGN_LAST_SENT_DATE";

                        SqlCommand command = new SqlCommand(sql, connection);

                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                        sqlDataAdapter.Fill(dt);
                        connection.Close();
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        var testingMode = ConfigurationManager.AppSettings.Get("TestingMode") ?? "true";
                        bool test = Convert.ToBoolean(testingMode);
                        foreach (DataRow dr in dt.Rows)
                        {
                            var newRow = ds.Tables[0].NewRow();
                            newRow["edm_name"] = dr["edm_name"];
                            newRow["title"] = dr["title"];
                            newRow["start_date"] = dr["start_date"];

                            string id = dr["id"].ToString();

                            if (test == true)
                            {
                                newRow["path"] = string.Format("http://10.90.101.33:8022/Preview.ashx?contentId={0}", id);//測試機 URL
                            }
                            else
                            {
                                newRow["path"] = string.Format("https://10.0.101.111:8022/Preview.ashx?contentId={0}", id);//正式機 URL
                            }

                            ds.Tables[0].Rows.InsertAt(newRow, 0);
                        }
                    }
                }
            }
            catch
            { }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
        /// <summary>
        /// 分店的最新公告
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage NewData([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            string sql;
            //2022/4/18 因信義店目前有四間分店，原本dic 只會帶出 sinyi，此參數只會撈取 A11 公告，沒有 A4, A8, A9，故需要調整成 union all
            if (dic != null && dic["dept"] == "sinyi")
            {
                sql = @"select top 8 * from (
                    select distinct top 8 * from(
                    select  'data\\svr2/sinyi/article/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyi_article_edm union
                    select 'data\\svr2/sinyi/BonusPenalty/Default.htm' as strfilepath, * from[001AAPW19].webmax6.webmax6.sinyi_BonusPenalty_edm union
                    select 'data\\svr2/sinyi/Other/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyi_Other_edm union
                    select 'data\\svr2/sinyi/SimpleNotes/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyi_SimpleNotes_edm
                    ) a order by start_date DESC
                    union all
                    select distinct top 8 * from(
                    select  'data\\svr2/sinyiA4/article/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA4_article_edm union
                    select 'data\\svr2/sinyiA4/BonusPenalty/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA4_BonusPenalty_edm union
                    select 'data\\svr2/sinyiA4/Other/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA4_Other_edm union
                    select 'data\\svr2/sinyiA4/SimpleNotes/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA4_SimpleNotes_edm
                    ) b order by start_date DESC
                    union all
                    select distinct top 8 * from(
                    select  'data\\svr2/sinyiA8/article/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA8_article_edm union
                    select 'data\\svr2/sinyiA8/BonusPenalty/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA8_BonusPenalty_edm union
                    select 'data\\svr2/sinyiA8/Other/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA8_Other_edm union
                    select 'data\\svr2/sinyiA8/SimpleNotes/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA8_SimpleNotes_edm
                    ) c order by start_date DESC
                    union all
                    select distinct top 8 * from(
                    select  'data\\svr2/sinyiA9/article/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA9_article_edm union
                    select 'data\\svr2/sinyiA9/BonusPenalty/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA9_BonusPenalty_edm union
                    select 'data\\svr2/sinyiA9/Other/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA9_Other_edm union
                    select 'data\\svr2/sinyiA9/SimpleNotes/Default.htm' as strfilepath, * from [001AAPW19].webmax6.webmax6.sinyiA9_SimpleNotes_edm
                    ) d order by start_date DESC
                    ) xinyi order by start_date DESC";
            }
            else
            {
                sql = "select distinct top 8 * from (" +
                "select  'data\\svr2/"+dic["dept"]+ "/article/Default.htm' as strfilepath,* from [001AAPW19].webmax6.webmax6." + dic["dept"]+"_article_edm union" +
                " select 'data\\svr2/"+dic["dept"]+ "/BonusPenalty/Default.htm' as strfilepath,* from [001AAPW19].webmax6.webmax6." + dic["dept"] + "_BonusPenalty_edm union" +
                " select 'data\\svr2/"+dic["dept"]+ "/Other/Default.htm' as strfilepath,* from [001AAPW19].webmax6.webmax6." + dic["dept"] + "_Other_edm union" +
                " select 'data\\svr2/"+dic["dept"]+ "/SimpleNotes/Default.htm' as strfilepath,* from [001AAPW19].webmax6.webmax6." + dic["dept"]+"_SimpleNotes_edm" +
                " ) a order by start_date DESC";
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sql, connection);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string path = dr["strfilepath"].ToString();
                    string id = dr["id"].ToString();
                    string newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", path.Substring(0, path.Length - 11), id);
                    //http://od-paper.skm.com.tw/' + val.strfilepath.substring(0, val.strfilepath.length - 11) + '/wmx_edmimage/' + val.id + '/edm_content.htm',
                    dr["strfilepath"] = newPath;
                }
            }
            catch
            {
                //20230802,避免舊系統沒資料,造成ds沒table,而讓後續新版edm無法填入,故填充一個ds中的table
                DataTable edmTable = ds.Tables.Add("Table");
                DataColumn pkEdmID =
                edmTable.Columns.Add("strfilepath", typeof(string));
                edmTable.Columns.Add("id", typeof(string));
                edmTable.Columns.Add("type", typeof(string));
                edmTable.Columns.Add("edm_name", typeof(string));
                edmTable.Columns.Add("title", typeof(string));
                edmTable.Columns.Add("start_date", typeof(string));
                edmTable.Columns.Add("end_date", typeof(string));
                edmTable.Columns.Add("period", typeof(string));
                edmTable.Columns.Add("status", typeof(string));
                edmTable.Columns.Add("publisher", typeof(string));
                edmTable.Columns.Add("rejectbox", typeof(string));
                edmTable.Columns.Add("duration", typeof(string));
                edmTable.Columns.Add("send_mode", typeof(string));
                edmTable.Columns.Add("target", typeof(string));
                edmTable.Columns.Add("faxperiod", typeof(string));
                edmTable.Columns.Add("publisher_fax_no", typeof(string));
                edmTable.Columns.Add("char_index", typeof(string));
                edmTable.Columns.Add("letters", typeof(string));
                edmTable.Columns.Add("accounting_pk", typeof(string));
                edmTable.Columns.Add("period_type", typeof(string));
                edmTable.Columns.Add("var_report", typeof(string));
                edmTable.Columns.Add("client_report", typeof(string));
                edmTable.Columns.Add("self_smtp", typeof(string));
                edmTable.Columns.Add("dns_sorting", typeof(string));
                edmTable.Columns.Add("isUseBlackList", typeof(string));
                edmTable.Columns.Add("disable_rule", typeof(string));
                edmTable.Columns.Add("img_link_flag", typeof(string));
                edmTable.Columns.Add("enable_probe", typeof(string));
                edmTable.Columns.Add("probe_mode", typeof(string));
                edmTable.Columns.Add("mail_rate", typeof(string));
                edmTable.Columns.Add("list_no", typeof(string));
                edmTable.Columns.Add("list_ruleid", typeof(string));
                edmTable.Columns.Add("email_fieldID", typeof(string));
                edmTable.Columns.Add("name_fieldID", typeof(string));
                edmTable.Columns.Add("proc_step", typeof(string));
                edmTable.Columns.Add("mail_size", typeof(string));
                edmTable.Columns.Add("normal_nail", typeof(string));
                edmTable.Columns.Add("test_nail", typeof(string));
                edmTable.Columns.Add("fetch_time", typeof(string));
                edmTable.Columns.Add("priority_level", typeof(string));
                edmTable.Columns.Add("expired_flag", typeof(string));
                edmTable.Columns.Add("isusehblist", typeof(string));
                edmTable.Columns.Add("isuseunsublist", typeof(string));
                edmTable.Columns.Add("unsub_epaper", typeof(string));

                edmTable.PrimaryKey = new DataColumn[] { pkEdmID };
            }

            //將該店的四項資訊全部撈進來
            //2022/6/20 對照 article = 函, simpleNotes = 簡便行文, bonusPenalty = 令, other = 其他
            //新 DM 系統邏輯，只要將 dic 進行轉換(店英文名換成代碼即可)
            try
            {
                var storeids = StoreEngNameToShopStoreId(dic);
                if (storeids.Count > 0)
                {
                    var dt = new DataTable();
                    using (SqlConnection connection = new SqlConnection(NewsLetterConnectionString))
                    {
                        connection.Open();
                        sql = $@"SELECT co.CAMPAIGN_NAME AS [edm_name], co.CAMPAIGN_TITLE AS [title], '' AS [strfilepath], co.CONTENT_ID AS [id],
                            ISNULL(co.CAMPAIGN_LAST_SENT_DATE, co.UPDATED_TIME) AS [start_date]
                            FROM CAMPAIGN_CATEGORY c INNER JOIN CAMPAIGN_CONTENT co ON c.CATEOGRY_ID = co.CATEOGRY_ID
                            WHERE c.STORE_ID IN ('{string.Join("','", storeids)}') AND c.CATEGORY_STATUS = 1 AND co.CAMPAIGN_STATUS > 0
                            ORDER BY co.CAMPAIGN_LAST_SENT_DATE";

                        SqlCommand command = new SqlCommand(sql, connection);

                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                        sqlDataAdapter.Fill(dt);
                        connection.Close();
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        var testingMode = ConfigurationManager.AppSettings.Get("TestingMode") ?? "true";
                        bool test = Convert.ToBoolean(testingMode);
                        foreach (DataRow dr in dt.Rows)
                        {
                            var newRow = ds.Tables[0].NewRow();
                            newRow["edm_name"] = dr["edm_name"];
                            newRow["title"] = dr["title"];
                            newRow["start_date"] = dr["start_date"];

                            string id = dr["id"].ToString();

                            if (test == true)
                            {
                                newRow["strfilepath"] = string.Format("http://10.90.101.33:8022/Preview.ashx?contentId={0}", id);//測試機 URL
                            }
                            else
                            {
                                newRow["strfilepath"] = string.Format("https://10.0.101.111:8022/Preview.ashx?contentId={0}", id);//正式機 URL
                            }

                            ds.Tables[0].Rows.InsertAt(newRow, 0);
                        }
                    }
                }
            }
            catch
            { }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
        /// <summary>
        /// 部門的最新消息
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeptNewData([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "Select * From News Where convert(varchar(8),getdate(),112) between start_date and end_date and type = 'msg' and dept=@dept "+
                                "order by Priority, create_date desc ";
                SqlCommand command = new SqlCommand(sql, connection);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] });

                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
    }
}
