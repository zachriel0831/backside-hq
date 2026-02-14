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
using System.Text.RegularExpressions;

namespace HQ.Controllers
{
    public class ValuesController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;


        [HttpPost]
        public HttpResponseMessage Read([FromBody]Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("select * from StoreTel where store_no not in ('160','162') order by store_no ", connection);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
        [HttpPost]
        public HttpResponseMessage Search([FromBody]Dictionary<string, string> dic)
        {

            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                /// TODO SQL
                /// 
                if (!dic.ContainsKey("bu"))
                {
                    string Sql_string = "select a.atitln,a.deptnm,a.epynm,a.pernr,b.tel ,right('00000000' + b.pernr ,8) from " +
                                    //   " zhrmaster a with(nolock)" +
                                     " [10.1.101.69].SKM.dbo.zhrmaster a with(nolock)" +
                                    "left join emp_tel b with(nolock) on a.pernr = right('00000000' + cast(b.pernr as varchar), 8) where deptnm like @name or b.tel like @name or epynm like @name"
                                    + @"  ORDER BY CASE WHEN a.atitln like '協理' then 1
				                                        WHEN a.atitln like '經理' then 2
				                                        WHEN a.atitln like '副理' then 3
				                                        WHEN a.atitln like '課長' then 4
				                                        WHEN a.atitln like '__專員' then 5
				                                        WHEN a.atitln like '組長' then 6
				                                        WHEN a.atitln like '班長' then 7
				                                        WHEN a.atitln like '專員' then 8
				                                        ELSE 9                      end";

                    SqlCommand command = new SqlCommand(Sql_string, connection);
                    command.Parameters.Add(new SqlParameter() { ParameterName = "@name", Value = "%" + dic["tel"] + "%" });
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }
                else
                {
                    string Sql = SqlString(dic);
                    SqlCommand command = new SqlCommand(Sql, connection);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();

                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }


        /// <summary>
        /// dic  bu ad_dept
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public String SqlString([FromBody]Dictionary<string, string> dic)
        {
            string sql = null;
            if (dic["bu"] == "本部" || dic["bu"] == "台北")
            {

                sql = "  select m.dept,m.epynm ,m.shopnm , m.atitln,m.deptcd,m.stell,m.email ,m.pernr,t.tel,m.deptnm from ";
                sql = sql + "(select dept,epynm ,shopnm , atitln,deptcd,stell,email ,pernr,deptnm,shopcd from (select ";


                if (dic["ad_dept"] == "稽核室")
                {

                    sql = sql + " case when deptnm like '%稽核室%' then '稽核室' ";
                }
                if (dic["ad_dept"] == "營業本部")
                {
                    sql = sql + " case when deptnm like ' % 營業本部 % ' then '營業本部' ";
                }

                if (dic["ad_dept"] == "經營企劃室")
                {
                    sql = sql + " case when deptnm like '%經營企劃%' then '經營企劃室' ";
                }

                if (dic["ad_dept"] == "事業本部")
                {
                    sql = sql + " case when deptnm like '%事業本部%' then '大陸事業本部' ";
                }

                if (dic["ad_dept"] == "新店舖籌備處")
                {
                    sql = sql + " case when deptnm like '%新店舖籌備處%' then '投資事業本部_店舖籌備' ";
                }

                if (dic["ad_dept"] == "新緯實")
                {
                    sql = sql + " case when deptnm like '%新緯實業%' then '投資事業本部_新緯' ";
                }


                if (dic["ad_dept"] == "秘書室")
                {
                    sql = sql + " case when deptnm like '%秘書室%' then '秘書室' ";
                }


                if (dic["ad_dept"] == "人力資源部" || dic["ad_dept"] == "人資部人事" || dic["ad_dept"] == "人資部顧服" || dic["ad_dept"] == "人資部教")
                {
                    sql = sql + " case when deptnm like '%人力資源%' then '人力資源部' ";
                    sql = sql + " when deptnm like '%人資部人事%' then '人力資源部' ";
                    sql = sql + " when deptnm like '%人資部顧服%' then '人力資源部' ";
                    sql = sql + " when deptnm like '%人資部教育%' then '人力資源部' ";
                }

                if (dic["ad_dept"] == "財務部")
                {
                    sql = sql + " case when deptnm like '%財務部%' then '財務部' ";
                    sql = sql + " when deptnm like '%財務部會計%' then '財務部' ";
                    sql = sql + " when deptnm like '%財務部法務%' then '財務部' ";
                    sql = sql + "	when deptnm like '%財務部出納%' then '財務部' ";
                }


                if (dic["ad_dept"] == "資訊部")
                {
                    sql = sql + "	case when deptnm like '%資訊部%' then '資訊部' ";
                    sql = sql + "	when deptnm like '%資訊部開發%' then '資訊部' ";
                    sql = sql + "	when deptnm like '%資訊部運用%' then '資訊部' ";
                    sql = sql + "	when deptnm like '%資訊部OA%' then '資訊部' ";
                }



                if (dic["ad_dept"] == "總務部")
                {
                    sql = sql + "	case when deptnm like '%總務部%' then '總務部' ";

                    sql = sql + " when deptnm like '%總務部總務%' then '總務部' ";

                    sql = sql + " when deptnm like '%總務部工務%' then '總務部' ";

                }

                //20240516新增投資管理部
                if (dic["ad_dept"] == "投資管理部")
                {
                    sql = sql + " case when deptnm like '%投資管理%' then '投資管理部' ";
                }

                //20240516新增數位發展部
                if (dic["ad_dept"] == "數位發展部")
                {
                    sql = sql + " case when deptnm like '%數位發展%' then '數位發展部' ";
                }

                //20240516新增自營事業部
                if (dic["ad_dept"] == "自營事業部")
                {
                    sql = sql + " case when deptnm like '%自營事業%' then '自營事業部' ";
                }

                //20240516新增電子商務部
                if (dic["ad_dept"] == "電子商務部")
                {
                    sql = sql + " case when deptnm like '%電子商務%' then '電子商務部' ";
                }

                //202400904新增顧客服務部
                if (dic["ad_dept"] == "顧客服務部")
                {
                    sql = sql + " case when deptnm like '%顧客服務%' then '顧客服務部' ";
                }

                if (dic["ad_dept"] == "投資事業本部")
                {
                    sql = sql + " case when deptnm like '%投資事業%' then '投資事業本部' ";
                }

                if (dic["ad_dept"] == "安控室")
                {
                    sql = sql + " case when deptnm like '%安控室%' then '安控室' ";
                }

                if (dic["ad_dept"] == "勞安衛生部")
                {
                    sql = sql + " case when deptnm like '%勞安衛生部%' then '勞安衛生部' ";
                }

                if (dic["ad_dept"] == "店舖開發部")
                {
                    sql = sql + " case when deptnm like '%店舖開發%' then '店舖開發部' ";
                }


                if (dic["ad_dept"] == "販促部")
                {
                    sql = sql + " case when deptnm like '%販促部%' then '販促部' ";

                    sql = sql + " when deptnm like '%販促部網路%' then '販促部' ";

                    sql = sql + " when deptnm like '%販促部文化%' then '販促部' ";

                    sql = sql + " when deptnm like '%販促部顧客卡務%' then '販促部' ";

                    sql = sql + " when deptnm like '%販促部企劃公關%' then '販促部' ";

                    sql = sql + " when deptnm like '%販促部美工%' then '販促部' ";
                }


                if (dic["ad_dept"] == "店舖規劃部")
                {
                    sql = sql + " case when deptnm like '%店舖%' then '店舖規劃部' ";

                    sql = sql + " when deptnm like '%店舖設計%' then '店舖規劃'   ";
                }


                if (dic["ad_dept"] == "電影事業部")
                {
                    sql = sql + " case when deptnm like '%電影事業部%' then '電影事業部' ";
                }


                if (dic["ad_dept"] == "電影事業部台北")
                {
                    sql = sql + " case when deptnm like '%台北新光影城%' then '電影事業部台北' ";
                }



                if (dic["ad_dept"] == "商品部")
                {
                    sql = sql + " case when deptcd = '8500' then '商品部' ";
                }


                if (dic["ad_dept"] == "大陸事業本部")
                {
                    sql = sql + " case ";

                    sql = sql + " when deptcd = '1102' and  deptnm like '%溫江%' 	then '大陸事業本部_溫江' ";

                    sql = sql + " when deptcd = '1102' and  deptnm like '%北京%'   then '大陸事業本部_北京' ";

                    sql = sql + " when deptcd = '1102' and  deptnm like '%蘇州%'   then '大陸事業本部_蘇州' ";

                    sql = sql + " when deptcd = '1102' and  deptnm like '%大陸事業%'  then '大陸事業本部' ";


                }



                if (dic["ad_dept"] == "業務本部")
                {
                    sql = sql + " deptnm dept,";
                }
                else
                {
                    if (dic["ad_dept"].Trim().Length > 0)
                    {
                        sql = sql + " end dept,";
                    }
                    else
                    {
                        sql = sql + " '' dept,";
                    }
                }
                sql = sql + " epynm ,shopnm , atitln,deptcd,stell,email ,pernr,deptnm,case when werks = 'A001' then '01' else substring(werks,2,2) end shopcd ";
                sql = sql + " from [10.1.101.69].SKM.dbo.zhrmaster ";
                //sql = sql + " from zhrmaster ";
                sql = sql + " where isnull(leaved,'') = '' ";

                sql = sql + " and leaved IS NULL AND  ((styst IS NULL AND staysp IS NULL) OR (styst IS NOT NULL AND staysp IS NOT NULL AND styst < staysp AND staysp < GETDATE())) ";
                sql = sql + " and enkokb in ('2','3') ";
                sql = sql + " and shopnm = '總公司' ";
                sql = sql + " ) a ) m ";
                sql = sql + " left join emp_tel t ";
                sql = sql + " on m.pernr = t.pernr  and m.shopcd = t.shopcd  ";
                sql = sql + " where 1=1 ";
                sql = sql + " and isnull(t.tel,'') <> '' ";
                //sql = sql + " and b.tel <> 'NULL'    ";


                if (dic["ad_dept"] != "業務本部" && dic["ad_dept"] != "營業本部" && dic["ad_dept"].Length > 0)
                {

                    sql = sql + " and (m.dept like '%" + dic["ad_dept"] + "%' ) ";
                }


                if (dic["ad_dept"] == "業務本部")
                {
                    sql = sql + " and (m.deptnm like '%業務本部%' or m.deptnm like '%財務部%' or m.deptnm like '%人力資源部%' or m.deptnm like '%資訊部%' or m.deptnm like '%總務部%') ";
                }


                if (dic["ad_dept"] == "營業本部")
                {
                    sql = sql + " and (m.deptnm like '%營業本部%' or m.deptcd = '8500'  or m.deptnm like '%店舖規劃部%' or m.deptnm like '%販促部%' ) ";
                }


                sql = sql + " order by m.stell,m.pernr,m.deptcd ";
            };

            if (dic["bu"] != "本部" && dic["bu"] != "台北" && dic["bu"].Length > 0)
            {
                sql = " select m.dept,m.epynm ,m.shopnm , m.atitln,m.deptcd,m.stell,m.email ,m.pernr,t.tel ,m.deptnm from  ( ";

                
                if (dic.ContainsKey("ad_dept")&&(dic["ad_dept"] == "電影事業部台南" || dic["ad_dept"] == "電影事業部台中"))
                {

                    sql = sql + " select  dept,epynm ,shopnm , atitln,deptcd,stell,email ,pernr,deptnm,shopcd from ";
                }
                else
                {
                    sql = sql + " select dept,epynm ,shopnm , atitln,deptcd,stell,email ,pernr,deptnm,shopcd from ";

                }

                sql = sql + " (select deptnm,";

                if (dic.ContainsKey("ad_dept"))
                {
                    if (dic["ad_dept"].TrimEnd() == "店舖管理")
                    {
                        //sql = sql + " case when deptnm like '%服務%' then '店舖管理' ";

                        //sql = sql + " when deptnm like '%總機%' then '店舖管理' ";

                        //sql = sql + " when deptnm like '%顧客服務%' then '店舖管理' ";

                        sql = sql + "  case when deptnm like '%管理%' then '店舖管理' ";                       

                        sql = sql + " when deptnm like '%安全%' then '店舖管理' ";

                        sql = sql + " when deptnm like '%工務%' then '店舖管理' ";

                        sql = sql + " when deptnm like '%電機%' then '店舖管理' ";

                        sql = sql + " when deptnm like '%總務%' then '店舖管理' ";
                    }

                    if (dic["ad_dept"].TrimEnd() == "顧客服務")
                    {
                        sql = sql + " case when deptnm like '%服務%' then '顧客服務' ";

                        sql = sql + " when deptnm like '%總機%' then '顧客服務' ";

                        sql = sql + " when deptnm like '%顧客服務%' then '顧客服務' ";

                        sql = sql + " when deptnm like '%護士%' then '顧客服務' ";

                        sql = sql + " when deptnm like '%接待%' then '顧客服務' ";

                    }


                    if (dic["ad_dept"].TrimEnd() == "行銷")
                    {
                        sql = sql + " case when deptnm like '%企劃%' then '行銷' ";

                        sql = sql + " when deptnm like '%行銷%' then '行銷' ";

                        sql = sql + " when deptnm like '%美工%' then '行銷' ";

                        sql = sql + " when deptnm like '%視覺%' then '行銷' ";
                    }



                    if (dic["ad_dept"] == "財務")
                    {
                        sql = sql + " case when deptnm like '%出納%' then '財務' ";

                        sql = sql + " when deptnm like '%財務%' then '財務' ";

                        sql = sql + " when deptnm like '%收銀%' then '財務' ";

                        sql = sql + " when deptnm like '%系統%' then '財務' ";

                        sql = sql + " when deptnm like '%會計%' then '財務' ";

                    }



                    if (dic["ad_dept"].TrimEnd() == "人事")
                    {
                        sql = sql + " case when deptnm like '%人事%' then '人事' ";
                    }


                    if (dic["ad_dept"].TrimEnd() == "店長室")
                    {
                        sql = sql + " case when deptnm like '%店長室%' then '店長室' ";
                    }


                    if (dic["ad_dept"].TrimEnd() == "電影事業部台中")
                    {
                        sql = sql + " case when deptnm like '%台中新光影城%' then '電影事業部台中' ";

                    }



                    if (dic["ad_dept"].TrimEnd() == "電影事業部台南")
                    {
                        sql = sql + " case when deptnm like '%台南新光影城%' then '電影事業部台南' ";

                    }


                    if (dic["ad_dept"].TrimEnd() == "營業")
                    {
                        sql = sql + " case when deptcd = '1100' or deptcd = '1000' then '營業' ";
                    }


                    if (dic["ad_dept"].TrimEnd() == "營業")
                    {
                        sql = sql + " when deptnm like '%女性%' then '營業' ";

                        sql = sql + " when deptnm like '%女性雜貨%' then '營業' ";

                        sql = sql + " when deptnm like '%內睡衣-內衣%' then '營業' ";

                        sql = sql + " when deptnm like '%休閒%' then '營業' ";

                        sql = sql + " when deptnm like '%男性%' then '營業' ";

                        sql = sql + " when deptnm like '%活動%' then '營業' ";

                        sql = sql + " when deptnm like '%家庭用品%' then '營業' ";

                        sql = sql + " when deptnm like '%營業%' then '營業' ";//2022/4/24 補上營業

                        sql = sql + " when deptnm like '%F%' then '營業' ";//20240911 補上F樓
                    }

                    if (dic["ad_dept"].TrimEnd() == "勞安衛生")
                    {
                        sql = sql + " case when deptnm like '%勞安衛生%' then '勞安衛生' ";
                    }


                    if (dic["ad_dept"].TrimEnd().Length > 0)
                    {
                        sql = sql + " end dept ,";
                    }
                }
                else
                {

                    sql = sql + " '' dept ,";
                }


                sql = sql + " epynm ,shopnm , atitln,deptcd,stell ,email,pernr,tel,case when werks = 'A001' then '01' else substring(werks,2,2) end shopcd  ";
                sql = sql + " from [10.1.101.69].SKM.dbo.zhrmaster ";                
                //sql = sql + " from zhrmaster ";
                sql = sql + " where isnull(leaved,'') = '' ";

                sql = sql + " and leaved IS NULL AND  ((styst IS NULL AND staysp IS NULL) OR (styst IS NOT NULL AND staysp IS NOT NULL AND styst < staysp AND staysp < GETDATE()))  ";
                sql = sql + " and enkokb = '2' ";


                if (dic["bu"].Length > 0)
                {
                    sql = sql + " and shopnm like '%" + dic["bu"] + "%' ";
                }


                sql = sql + " ) a ) m ";
                sql = sql + " left join emp_tel t ";
                sql = sql + " on m.pernr = t.pernr ";
                sql = sql + " where 1=1 and isnull(t.tel,'') <> '' ";



                if (dic.ContainsKey("ad_dept"))
                {
                    sql = sql + " and dept like '%" + dic["ad_dept"] + "%' ";
                }



                sql = sql + @"  ORDER BY CASE WHEN m.atitln like '協理' then 1
				                              WHEN m.atitln like '經理' then 2
				                              WHEN m.atitln like '副理' then 3
				                              WHEN m.atitln like '課長' then 4
				                              WHEN m.atitln like '__專員' then 5
				                              WHEN m.atitln like '組長' then 6
				                              WHEN m.atitln like '班長' then 7
				                              WHEN m.atitln like '專員' then 8
				                              ELSE 9                      end"
                + " , m.stell,m.deptcd ,m.pernr ";


            }
            return sql;
        }
    }
}

