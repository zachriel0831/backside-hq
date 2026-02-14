using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace HQ.Controllers
{
    public class MainController : ApiController
    {
        private string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
        private string NewsLetterConnectionString = ConfigurationManager.ConnectionStrings["NewsLetter"].ConnectionString;

        public enum DeptType
        {
            None,
            HR,
            COM
        }

        private string DetectUtf8(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes ?? Array.Empty<byte>());
        }

        // 純文字 URL 備援（http/https）
        private static readonly Regex UrlRegex =
            new Regex(@"https?://[^\s<>)]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 純文字中抓「像檔案的 token」（含相對/絕對、query/fragment）
        private static readonly Regex FileLikeRegex = new Regex(
            @"(?<![\w/\.])(?<url>(?:https?://|//|/|\./|\.\./)?[^\s<>'"")\]]+?\.(?<ext>[A-Za-z0-9]{1,8})(?:\?[^\s<>'"")\]]*)?(?:#[^\s<>'"")\]]*)?)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private class Downloaded
        {
            public byte[] Bytes;
            public string ContentType;
            public string FinalUrl;
        }

        private class LinkDto
        {
            public string text;
            public string href;
            public string filename;
        }

        // 預設排除常見「頁面」副檔名
        private static readonly HashSet<string> DefaultExcludedPageExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "html","htm","php","asp","aspx","jsp","jspx","cfm","cgi"
        };

        // 常見白名單（可選用；預設不強制）
        private static readonly HashSet<string> DefaultCommonAllowedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // 先留pdf
            "pdf"
        };

        /// <summary>
        /// 解析指定 URL，下載 HTML 或 PDF，並擷取所有「附檔」連結資訊（不限副檔名）。
        /// 回傳格式： data { original_html, links: [{text, href, filename}] }
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> HtmlScraper([FromBody] Dictionary<string, string> dic)
        {
            try
            {
                var url = dic["url"];
                var downloaded = await DownloadAsync(url);

                List<LinkDto> links;
                string original = string.Empty;

                if (IsPdf(downloaded.ContentType, downloaded.Bytes))
                {
                    // 先擷取 PDF 純文字，回放到 original_html
                    original = ExtractPdfText(downloaded.Bytes);

                    // 抓 PDF 內嵌 URI 連結
                    links = ParsePdfLinks(downloaded.Bytes, downloaded.FinalUrl);

                    // 若沒抓到連結，再用純文字補抓
                    if (links.Count == 0)
                    {
                        links = LinksFromPlainAllFiles(original, downloaded.FinalUrl, DefaultCommonAllowedExts, DefaultExcludedPageExts);
                    }
                }
                else
                {
                    // HTML：放原始 HTML
                    var html = DetectUtf8(downloaded.Bytes);
                    original = html;

                    // 解析所有附檔連結
                    links = await ParseHtmlLinksWithAngleSharpAsync(
                        html,
                        downloaded.FinalUrl,
                        DefaultCommonAllowedExts,
                        DefaultExcludedPageExts,
                        default(CancellationToken));

                    if (links.Count == 0)
                    {
                        links = LinksFromPlainAllFiles(html, downloaded.FinalUrl, DefaultCommonAllowedExts, DefaultExcludedPageExts);
                    }
                }

                // 去重並組裝輸出
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var linkResults = new List<object>();
                foreach (var l in links ?? new List<LinkDto>())
                {
                    var href = l.href ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(href)) continue;
                    if (!seen.Add(href)) continue;

                    var filename = string.IsNullOrWhiteSpace(l.filename) ? FilenameFromUrl(href) : l.filename;
                    var text = string.IsNullOrWhiteSpace(l.text) ? filename : l.text;

                    linkResults.Add(new
                    {
                        text = text,
                        href = href,
                        filename = filename
                    });
                }

                var payload = new
                {
                    data = new
                    {
                        original_html = original,
                        links = linkResults
                    }
                };

                return Request.CreateResponse(HttpStatusCode.OK, payload);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    error = ex.Message,
                    stack = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// 使用 AngleSharp 解析 HTML，抓所有「看起來像檔案」的連結（不限副檔名）。
        /// 允許以 allowedExtensions 做白名單，或 excludedExtensions 擴充黑名單。
        /// </summary>
        private async Task<List<LinkDto>> ParseHtmlLinksWithAngleSharpAsync(
            string html,
            string baseUrl,
            ISet<string> allowedExtensions,
            ISet<string> excludedExtensions,
            CancellationToken ct)
        {
            var list = new List<LinkDto>();
            if (excludedExtensions == null) excludedExtensions = DefaultExcludedPageExts;

            var cfg = AngleSharp.Configuration.Default;
            var ctx = BrowsingContext.New(cfg);
            var doc = await ctx.OpenAsync(r => r.Content(html).Address(baseUrl), ct);

            // 1) <a href>
            foreach (var a in doc.QuerySelectorAll("a[href]"))
            {
                var raw = a.GetAttribute("href");
                raw = raw == null ? null : raw.Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                var abs = ToAbsoluteUrl(baseUrl, raw);
                if (string.IsNullOrEmpty(abs)) continue;

                if (!LooksLikeAttachment(abs, allowedExtensions, excludedExtensions, null)) continue;

                var text = NormalizeText((a as IHtmlAnchorElement) != null
                    ? ((IHtmlAnchorElement)a).TextContent
                    : a.TextContent);

                if (string.IsNullOrWhiteSpace(text)) text = FilenameFromUrl(abs);

                list.Add(new LinkDto { text = text, href = abs, filename = FilenameFromUrl(abs) });
            }

            // 2) 常見承載屬性
            var carriers = new Tuple<string, string>[]
            {
                Tuple.Create("object[data]", "data"),
                Tuple.Create("embed[src]",  "src"),
                Tuple.Create("iframe[src]", "src"),
                Tuple.Create("link[href]",  "href"),
                Tuple.Create("source[src]", "src")
            };

            foreach (var t in carriers)
            {
                string selector = t.Item1;
                string attr = t.Item2;

                foreach (var el in doc.QuerySelectorAll(selector))
                {
                    var raw = el.GetAttribute(attr);
                    raw = raw == null ? null : raw.Trim();
                    if (string.IsNullOrEmpty(raw)) continue;

                    var abs = ToAbsoluteUrl(baseUrl, raw);
                    if (string.IsNullOrEmpty(abs)) continue;

                    if (!LooksLikeAttachment(abs, allowedExtensions, excludedExtensions, null)) continue;

                    var text = NormalizeText(el.GetAttribute("title") ?? el.GetAttribute("aria-label") ?? string.Empty);
                    if (string.IsNullOrWhiteSpace(text)) text = FilenameFromUrl(abs);

                    list.Add(new LinkDto { text = text, href = abs, filename = FilenameFromUrl(abs) });
                }
            }

            // 3) 純文字補抓
            foreach (var node in doc.All.Where(n =>
                n.LocalName != "script" && n.LocalName != "style" &&
                !string.IsNullOrWhiteSpace(n.TextContent)))
            {
                var t = node.TextContent;
                foreach (Match m in FileLikeRegex.Matches(t))
                {
                    var rawUrl = TrimTrailingPunctuation(m.Groups["url"].Value);
                    var ext = m.Groups["ext"].Value;

                    var abs = ToAbsoluteUrl(baseUrl, rawUrl);
                    if (string.IsNullOrEmpty(abs)) continue;

                    if (!LooksLikeAttachment(abs, allowedExtensions, excludedExtensions, ext)) continue;

                    list.Add(new LinkDto
                    {
                        text = FilenameFromUrl(abs),
                        href = abs,
                        filename = FilenameFromUrl(abs)
                    });
                }
            }

            // 去重
            return list
                .GroupBy(x => x.href, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }

        /// <summary>
        /// 解析 PDF 檔案中的 URI 連結註解。
        /// </summary>
        private List<LinkDto> ParsePdfLinks(byte[] pdfBytes, string baseUrl)
        {
            var result = new List<LinkDto>();
            using (var reader = new PdfReader(pdfBytes))
            {
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var pageDict = reader.GetPageN(page);
                    var annots = pageDict.GetAsArray(PdfName.ANNOTS);
                    if (annots == null) continue;

                    foreach (var a in annots.ArrayList)
                    {
                        var annot = PdfReader.GetPdfObject(a) as PdfDictionary;
                        if (annot == null) continue;
                        if (!PdfName.LINK.Equals(annot.GetAsName(PdfName.SUBTYPE))) continue;

                        var action = annot.GetAsDict(PdfName.A);
                        if (action == null || !PdfName.URI.Equals(action.GetAsName(PdfName.S))) continue;

                        var uri = action.GetAsString(PdfName.URI);
                        if (uri == null) continue;

                        string href;
                        try { href = new Uri(new Uri(baseUrl), uri.ToString()).ToString(); }
                        catch { href = uri.ToString(); }

                        var text = FilenameFromUrl(href);
                        result.Add(new LinkDto { text = text, href = href, filename = FilenameFromUrl(href) });
                    }
                }
            }
            return result.GroupBy(x => x.href, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        }

        /// <summary>
        /// 擷取 PDF 內所有純文字內容（回退用）。
        /// </summary>
        private string ExtractPdfText(byte[] pdfBytes)
        {
            var sb = new StringBuilder();
            using (var reader = new PdfReader(pdfBytes))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    sb.AppendLine(PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy()));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 下載指定 URL 的內容。
        /// </summary>
        private async Task<Downloaded> DownloadAsync(string url)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("HtmlScraper/2.0");

                try
                {
                    var resp = await client.GetAsync(url);
                    resp.EnsureSuccessStatusCode();
                    return new Downloaded
                    {
                        Bytes = await resp.Content.ReadAsByteArrayAsync(),
                        ContentType = resp.Content.Headers.ContentType != null ? resp.Content.Headers.ContentType.ToString() : null,
                        FinalUrl = resp.RequestMessage != null && resp.RequestMessage.RequestUri != null
                            ? resp.RequestMessage.RequestUri.ToString()
                            : url
                    };
                }
                catch (TaskCanceledException ex)
                {
                    if (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("下載 URL 超時 (20秒): " + url, ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// 判斷是否 PDF（用於選擇 PDF/HTML 解析路徑）。
        /// </summary>
        private bool IsPdf(string contentType, byte[] bytes)
        {
            if (!string.IsNullOrEmpty(contentType) && contentType.IndexOf("pdf", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return bytes != null && bytes.Length >= 4 &&
                   bytes[0] == '%' && bytes[1] == (byte)'P' && bytes[2] == (byte)'D' && bytes[3] == (byte)'F';
        }

        /// <summary>
        /// 從純文字擷取所有「看起來像檔案」的 URL（含相對路徑），可傳 allowed/excluded 調整範圍。
        /// </summary>
        private List<LinkDto> LinksFromPlainAllFiles(string text, string baseUrl, ISet<string> allowedExtensions, ISet<string> excludedExtensions)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (excludedExtensions == null) excludedExtensions = DefaultExcludedPageExts;

            // 1) 先抓 http/https URL
            foreach (Match m in UrlRegex.Matches(text ?? string.Empty))
            {
                var abs1 = ToAbsoluteUrl(baseUrl, m.Value.Trim());
                if (LooksLikeAttachment(abs1, allowedExtensions, excludedExtensions, null))
                    set.Add(abs1);
            }

            // 2) 再抓「像檔案的 token」（含相對）
            foreach (Match m in FileLikeRegex.Matches(text ?? string.Empty))
            {
                var raw = TrimTrailingPunctuation(m.Groups["url"].Value);
                var ext = m.Groups["ext"].Value;
                var abs2 = ToAbsoluteUrl(baseUrl, raw);
                if (LooksLikeAttachment(abs2, allowedExtensions, excludedExtensions, ext))
                    set.Add(abs2);
            }

            return set.Select(h => new LinkDto
            {
                text = FilenameFromUrl(h),
                href = h,
                filename = FilenameFromUrl(h)
            }).ToList();
        }

        // ===== Helpers =====

        private static string ToAbsoluteUrl(string baseUrl, string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            try
            {
                var baseUri = new Uri(baseUrl, UriKind.Absolute);
                return new Uri(baseUri, raw).ToString();
            }
            catch
            {
                Uri u;
                if (Uri.TryCreate(raw, UriKind.Absolute, out u) &&
                    (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
                    return u.ToString();
                if (raw.StartsWith("//")) return "https:" + raw; // 保守補 https
                return raw; // 留待後續判斷
            }
        }

        private static bool LooksLikeAttachment(
            string url,
            ISet<string> allowedExts,
            ISet<string> excludedExts,
            string extHint)
        {
            var ext = extHint ?? GetExtensionFromUrl(url);
            if (string.IsNullOrEmpty(ext)) return false;

            if (excludedExts != null && excludedExts.Contains(ext)) return false; // 排除頁面副檔
            if (allowedExts != null) return allowedExts.Contains(ext);            // 有白名單則必須命中

            // 放寬判斷，但避免明顯誤抓
            if (ext.Length < 1 || ext.Length > 8) return false;
            bool allDigits = true;
            for (int i = 0; i < ext.Length; i++)
            {
                if (!char.IsDigit(ext[i])) { allDigits = false; break; }
            }
            if (allDigits) return false;

            return true;
        }

        private static string GetExtensionFromUrl(string url)
        {
            try
            {
                var clean = url.Split('#')[0].Split('?')[0];
                if (clean.EndsWith("/")) return string.Empty;
                var last = clean.Substring(clean.LastIndexOf('/') + 1);
                var dot = last.LastIndexOf('.');
                if (dot <= 0 || dot == last.Length - 1) return string.Empty;
                return last.Substring(dot + 1).ToLowerInvariant();
            }
            catch { return string.Empty; }
        }

        private static string NormalizeText(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = WebUtility.HtmlDecode(s);
            s = s.Replace('\u00A0', ' ').Replace('\u3000', ' ');
            s = Regex.Replace(s, @"\s+", " ").Trim();
            // 收斂中日韓/字母/數字間多餘空白
            s = Regex.Replace(s, @"(?<=[\p{IsCJKUnifiedIdeographs}A-Za-z0-9])\s+(?=[\p{IsCJKUnifiedIdeographs}A-Za-z0-9])", "");
            return s;
        }

        private static string TrimTrailingPunctuation(string s)
        {
            return s.TrimEnd(new[] { '.', ',', ';', ':', ')', ']', '}', '»', '"', '\'', '、', '。' });
        }

        private string FilenameFromUrl(string href)
        {
            try
            {
                var path = new Uri(href).AbsolutePath;
                var name = path.Substring(path.LastIndexOf('/') + 1);
                return string.IsNullOrWhiteSpace(name) ? "file" : name;
            }
            catch
            {
                var i = href.LastIndexOf('/');
                return i >= 0 ? href.Substring(i + 1) : "file";
            }
        }

        [HttpPost]
        public HttpResponseMessage Read([FromBody] Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            DataSet campaign = Campaign(dic);
            string dept = "skmit_" + campaign.Tables[0].Rows[0].ItemArray[0] + "_edm";

            DeptType deptType = DeptType.None;
            if (dic != null && dic.ContainsKey("dept"))
            {
                string deptText = dic["dept"];
                if (!string.IsNullOrEmpty(deptText))
                {
                    if (deptText.ToLower().Contains("hr"))
                        deptType = DeptType.HR;
                    else if (deptText.ToLower().Contains("com"))
                        deptType = DeptType.COM;
                }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "select distinct top 8 * from (" +
                        "select  p.edm_name , p.title , len(p.title) as t ,@path as path, " +
                        " p.id , p.start_date , a.attach_no , a.urlpath as apath " +
                        "from [001AAPW19].webmax6.webmax6." + dept + " as p left join Attach as a on a.edm_name = p.edm_name ) a order by start_date DESC";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.Add(new SqlParameter() { ParameterName = "@path", Value = campaign.Tables[0].Rows[0].ItemArray[1] });

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                //系統通訊和 HR 的 title 有長度差異，呈現的 HTML 也不相同
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string path = dr["path"].ToString();
                    string id = dr["id"].ToString();

                    string newPath = path;
                    if (deptType == DeptType.HR)
                    {
                        newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", path.Substring(0, path.Length - 11), id);
                        //'http://od-paper.skm.com.tw/' + val.path.substring(0, val.path.length - 11) + "/wmx_edmimage/" + val.id + '/edm_content.htm'
                        string title = dr["title"].ToString();
                        int length = 16;
                        if (title.Length < 17)
                            length = title.Length;
                        title = $"◎{title.Substring(0, length)}...";
                        dr["title"] = title;
                    }
                    else if (deptType == DeptType.COM)
                    {
                        newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", path.Replace("/Default.htm", ""), id);
                        string title = dr["title"].ToString();
                        int length = 44;
                        if (title.Length < 45)
                        {
                            if (title.Length - 13 < 30)
                                length = title.Length - 13;
                            else
                                length = 20;
                        }
                        else
                            length = 20;
                        title = $"◎{title.Substring(13, length)}...";
                        dr["title"] = title;
                        //    'href': 'http://od-paper.skm.com.tw/' + val.path.replace("/Default.htm", "") + "/wmx_edmimage/" + val.id + "/edm_content.htm",
                        //'html': '◎ ' + val.title.substring(13, 30) + '...',
                    }

                    dr["path"] = newPath;
                }
            }
            catch
            { }

            try
            {
                string categoryName = string.Empty;

                if (deptType == DeptType.HR)
                {
                    categoryName = "97公文發佈";
                }
                else if (deptType == DeptType.COM)
                {
                    categoryName = "98系統通訊";
                }

                if (!string.IsNullOrEmpty(categoryName))
                {
                    string sql = $@"SELECT TOP 8 
                            co.CAMPAIGN_NAME AS [edm_name], co.CAMPAIGN_TITLE AS [title], 
                            LEN(co.CAMPAIGN_TITLE) AS [t], '' AS [path], co.CONTENT_ID AS [id],
                            ISNULL(co.CAMPAIGN_LAST_SENT_DATE, co.UPDATED_TIME) AS [start_date]
                            FROM CAMPAIGN_CATEGORY c INNER JOIN CAMPAIGN_CONTENT co ON c.CATEOGRY_ID = co.CATEOGRY_ID
                            WHERE c.STORE_ID = '010' AND c.CATEGORY_NAME = N'{categoryName}' AND c.CATEGORY_STATUS = 1 AND co.CAMPAIGN_SENT_MODE = 1 
                            AND co.CAMPAIGN_STATUS > 0 AND co.CAMPAIGN_LAST_SENT_STATUS IS NOT NULL 
                            AND isnull(co.CAMPAIGN_SCHEDULTED_DATE,GETDATE()) <= GETDATE()
                            ORDER BY [start_date] desc";

                    var dt = new DataTable();
                    using (SqlConnection connection = new SqlConnection(NewsLetterConnectionString))
                    {
                        connection.Open();
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

                            string title = dr["title"].ToString();
                            if (deptType == DeptType.HR)
                            {
                                int length = 16;
                                if (title.Length < 17)
                                    length = title.Length;
                                title = $"◎{title.Substring(0, length)}...";
                            }
                            else if (deptType == DeptType.COM)
                            {
                                int length = 18; //20220823 原本設定14
                                if (title.Length < 19) //20220823 原本設定<15
                                    length = title.Length;
                                title = $"◎{title.Substring(0, length)}...";
                            }
                            newRow["title"] = title;

                            newRow["t"] = dr["t"];
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

                        //20220812 Weily, 資料組合後,資料排序有問題,先轉成DataView進行排序
                        DataView dv = new DataView(ds.Tables[0], "", "start_date desc", DataViewRowState.CurrentRows);
                        //將原本的DataSet清空
                        ds = new DataSet();
                        //把排序好的dv放回DataSet中,這動作主要搭配後續程式,免得變動太多
                        ds.Tables.Add(dv.ToTable());                       

                        if (ds.Tables[0].Rows.Count > 8)
                        {
                            int iCount = ds.Tables[0].Rows.Count; //取得目前共幾筆資料

                            //20220812 Weily ,針對每一次的第9筆刪除
                            for (int i = 0; i < (iCount -8); i++)
                            {
                                try
                                {
                                    DataRow dr = ds.Tables[0].Rows[8]; //永遠刪除第9筆,前8筆保留
                                    dr.Delete();
                                }
                                catch { }
                            }
                        }
                        ds.Tables[0].AcceptChanges(); //實際變更
                    }
                }
            }
            catch
            { }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
        /// <summary>
        /// to get campaign_id homepage startdate with skmit_campaign
        /// </summary>
        /// <param name="dic">department</param>
        /// <returns></returns>
        private DataSet Campaign([FromBody] Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();

            //奕丞 2022/4/18 加入 cache 機制
            ObjectCache cache = MemoryCache.Default;
            string cacheKey = $"Campaign_{dic["dept"]}";
            var objects = cache[cacheKey];
            if (objects != null && objects is DataSet set)
            {
                ds = set;
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("select distinct campaign_id,homepage,startdate from [skmit_campaign] where campaign_id like @dept order by startdate desc", connection);
                    command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] + "%" });
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                DateTime expiredTime = DateTime.Now.Add(new TimeSpan(3, 0, 0));
                DateTimeOffset timeOffset = new DateTimeOffset(expiredTime);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = timeOffset
                };
                if (cache.Contains(cacheKey))
                    cache.Remove(cacheKey);
                cache.Add(cacheKey, ds, policy);
            }
            return ds;
        }

        [HttpPost]
        public HttpResponseMessage WebConnectData([FromBody] Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();

            //奕丞 2022/4/18 加入 cache 機制
            ObjectCache cache = MemoryCache.Default;
            string cacheKey = "WebConnectData";
            var objects = cache[cacheKey];
            if (objects != null && objects is DataSet set)
            {
                ds = set;
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "Select * From News Where convert(varchar(8),getdate(),112) between start_date and end_date and type = 'link' order by Priority, create_date desc ";
                    SqlCommand command = new SqlCommand(sql, connection);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                DateTime expiredTime = DateTime.Now.Add(new TimeSpan(12, 0, 0));
                DateTimeOffset timeOffset = new DateTimeOffset(expiredTime);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = timeOffset
                };
                if (cache.Contains(cacheKey))
                    cache.Remove(cacheKey);
                cache.Add(cacheKey, ds, policy);
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
        /// <summary>
        /// Safe data  *** to do with official Sql server ***
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SafeRead([FromBody] Dictionary<string, string> dic)
        {
            //2022/4/25 奕丞 連線字串一律使用 config 檔案，不需單獨 hard code
            //string ConnectionSafeString = "Data Source = 10.1.101.52; Initial Catalog = portal; User ID = portaladmin; Password = skmportaladmin;";
            DataSet ds = new DataSet();

            //奕丞 2022/4/18 加入 cache 機制
            ObjectCache cache = MemoryCache.Default;
            string cacheKey = "SafeRead";
            var objects = cache[cacheKey];
            if (objects != null && objects is DataSet set && 1==0)
            {
                ds = set;
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "select * from ( select  case when dept = '案例分享' then 1 when dept = '最新公告' then 2  " +
                                "when dept = '安全新知' then 3 end depti, " +
                                "ROW_NUMBER() OVER(PARTITION BY dept ORDER BY priority ASC) AS 'RowNO', dept, background title, des_no," +
                                "CONVERT(varchar,create_date,112) DT from News with(nolock) where type = 'msg1' ) m " +
                                "where RowNO <= '2' order by depti,RowNO";
                    SqlCommand command = new SqlCommand(sql, connection);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                DateTime expiredTime = DateTime.Now.Add(new TimeSpan(12, 0, 0));
                DateTimeOffset timeOffset = new DateTimeOffset(expiredTime);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = timeOffset
                };
                if (cache.Contains(cacheKey))
                    cache.Remove(cacheKey);
                cache.Add(cacheKey, ds, policy);
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        /// <summary>
        /// 20221004,Weily Add 人資園地
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage HrRead([FromBody] Dictionary<string, string> dic)
        {
            //2022/4/25 奕丞 連線字串一律使用 config 檔案，不需單獨 hard code
            //string ConnectionSafeString = "Data Source = 10.1.101.52; Initial Catalog = portal; User ID = portaladmin; Password = skmportaladmin;";
            DataSet ds = new DataSet();

            //奕丞 2022/4/18 加入 cache 機制
            ObjectCache cache = MemoryCache.Default;
            string cacheKey = "HrRead";
            var objects = cache[cacheKey];
            if (objects == null && objects is DataSet set && 1==0)
            {
                ds = set;
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "select * from ( select  case when dept = '政策推動' then 1 when dept = '熱門話題' then 2  " +
                                "when dept = '職場生活' then 3 when dept = '快樂員購' then 4 end depti, " +
                                "ROW_NUMBER() OVER(PARTITION BY dept ORDER BY priority ASC) AS 'RowNO', dept, background title, des_no," +
                                "CONVERT(varchar,create_date,112) DT from News with(nolock) where type = 'msg2' ) m " +
                                "where RowNO <= '2' order by depti,RowNO";
                    SqlCommand command = new SqlCommand(sql, connection);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                DateTime expiredTime = DateTime.Now.Add(new TimeSpan(12, 0, 0));
                DateTimeOffset timeOffset = new DateTimeOffset(expiredTime);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = timeOffset
                };
                if (cache.Contains(cacheKey))
                    cache.Remove(cacheKey);
                cache.Add(cacheKey, ds, policy);
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }


        /// <summary>
        /// Get all Announcement content 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage HrData([FromBody] Dictionary<string, string> dic)
        {
            //此 function 的 dic 只會傳入 hr
            DataSet ds = new DataSet();
            DataSet campaign = Campaign(dic);
            string dept = "skmit_" + campaign.Tables[0].Rows[0].ItemArray[0] + "_edm";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "select  p.edm_name , p.title , len(p.title) as t ,@path as path, " +
                        " p.id , p.start_date , a.attach_no , a.urlpath as apath " +
                        " from [001AAPW19].webmax6.webmax6." + dept + " as p left join Attach as a on a.edm_name = p.edm_name where p.start_date >= '2013/1/1' order by start_date DESC";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.Add(new SqlParameter() { ParameterName = "@path", Value = campaign.Tables[0].Rows[0].ItemArray[1] });

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string path = dr["path"].ToString();
                    string id = dr["id"].ToString();
                    string newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", path.Substring(0, path.Length - 11), id);
                    //'http://od-paper.skm.com.tw/' + val.path.substring(0, val.path.length - 11) + "/wmx_edmimage/" + val.id + '/edm_content.htm'
                    dr["path"] = newPath;
                }
            }
            catch
            { }

            try
            {
                var dt = new DataTable();
                using (SqlConnection connection = new SqlConnection(NewsLetterConnectionString))
                {
                    connection.Open();
                    string sql = @"SELECT 
                            co.CAMPAIGN_NAME AS [edm_name], co.CAMPAIGN_TITLE AS [title], 
                            LEN(co.CAMPAIGN_TITLE) AS [t], '' AS [path], co.CONTENT_ID AS [id],
                            ISNULL(co.CAMPAIGN_LAST_SENT_DATE, co.UPDATED_TIME) AS [start_date]
                            FROM CAMPAIGN_CATEGORY c INNER JOIN CAMPAIGN_CONTENT co ON c.CATEOGRY_ID = co.CATEOGRY_ID
                            WHERE c.STORE_ID = '010' AND c.CATEGORY_NAME = '97公文發佈' AND c.CATEGORY_STATUS = 1
                            AND co.CAMPAIGN_STATUS > 0 AND co.CAMPAIGN_LAST_SENT_STATUS IS NOT NULL 
                            AND isnull(co.CAMPAIGN_SCHEDULTED_DATE,GETDATE()) <= GETDATE()
                            ORDER BY [start_date] ";
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
                        newRow["t"] = dr["t"];
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
            catch
            { }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }


        /// <summary>
        /// Get all Announcement content 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage ItData([FromBody] Dictionary<string, string> dic)
        {
            //此 function 的 dic 會傳入 com or hr
            DataSet ds = new DataSet();
            DataSet campaign = Campaign(dic);
            string dept = "skmit_" + campaign.Tables[0].Rows[0].ItemArray[0] + "_edm";

            DeptType deptType = DeptType.None;
            if (dic != null && dic.ContainsKey("dept"))
            {
                string deptText = dic["dept"];
                if (!string.IsNullOrEmpty(deptText))
                {
                    if (deptText.ToLower().Contains("hr"))
                        deptType = DeptType.HR;
                    else if (deptText.ToLower().Contains("com"))
                        deptType = DeptType.COM;
                }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "select  p.edm_name , substring(p.title,1,11) as title , substring(title,14,45) as t,len(title) as t1 ,@path as path, " +
                        " p.id , p.start_date , a.attach_no , a.urlpath as apath " +
                        "from [001AAPW19].webmax6.webmax6." + dept + " as p left join Attach as a on a.edm_name = p.edm_name where p.start_date >= '2013/1/1' order by start_date DESC";
                    if (dic["dept"] == "hr")
                    {
                        sql = "select  p.edm_name , p.title , len(p.title) as t ,@path as path, " +
                        " p.id , p.start_date , a.attach_no , a.urlpath as apath " +
                        "from [001AAPW19].webmax6.webmax6." + dept + " as p left join Attach as a on a.edm_name = p.edm_name where p.start_date >= '2013/1/1' order by start_date DESC";
                    }
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.Add(new SqlParameter() { ParameterName = "@path", Value = campaign.Tables[0].Rows[0].ItemArray[1] });

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string path = dr["path"].ToString();
                    string id = dr["id"].ToString();
                    string newPath = path;

                    if (deptType == DeptType.HR)
                    {
                        newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", path.Substring(0, path.Length - 11), id);
                    }
                    else if (deptType == DeptType.COM)
                    {
                        newPath = string.Format("{0}/{1}/wmx_edmimage/{2}/edm_content.htm", "http://od-paper.skm.com.tw", path.Replace("/Default.htm", ""), id);
                    }
                    dr["path"] = newPath;
                }
            }
            catch
            { }

            try
            {
                string sql = string.Empty;

                if (deptType == DeptType.HR)
                {
                    sql = @"SELECT co.CAMPAIGN_NAME AS [edm_name], co.CAMPAIGN_TITLE AS [title], 
                            LEN(co.CAMPAIGN_TITLE) AS [t], '' AS [path], co.CONTENT_ID AS [id],
                            ISNULL(co.CAMPAIGN_LAST_SENT_DATE, co.UPDATED_TIME) AS [start_date]
                            FROM CAMPAIGN_CATEGORY c INNER JOIN CAMPAIGN_CONTENT co ON c.CATEOGRY_ID = co.CATEOGRY_ID
                            WHERE c.STORE_ID = '010' AND c.CATEGORY_NAME = N'97公文發佈' AND c.CATEGORY_STATUS = 1 AND co.CAMPAIGN_SENT_MODE = 1 
                            AND co.CAMPAIGN_STATUS > 0 AND co.CAMPAIGN_LAST_SENT_STATUS IS NOT NULL 
                            AND isnull(co.CAMPAIGN_SCHEDULTED_DATE,GETDATE()) <= GETDATE()
                            ORDER BY [start_date]";
                }
                else if (deptType == DeptType.COM)
                {
                    //20220823原本SUBSTRING(co.CAMPAIGN_TITLE, 14, 45) AS [t],後改co.CAMPAIGN_TITLE
                    sql = @"SELECT co.CAMPAIGN_NAME AS [edm_name], co.CAMPAIGN_TITLE AS [title], 
                            co.CAMPAIGN_TITLE AS [t], '' AS [path], co.CONTENT_ID AS [id],
                            LEN(co.CAMPAIGN_TITLE) AS [t1], ISNULL(co.CAMPAIGN_LAST_SENT_DATE, co.UPDATED_TIME) AS [start_date]
                            FROM CAMPAIGN_CATEGORY c INNER JOIN CAMPAIGN_CONTENT co ON c.CATEOGRY_ID = co.CATEOGRY_ID
                            WHERE c.STORE_ID = '010' AND c.CATEGORY_NAME = N'98系統通訊' AND c.CATEGORY_STATUS = 1 AND co.CAMPAIGN_SENT_MODE = 1 
                            AND co.CAMPAIGN_STATUS > 0 AND co.CAMPAIGN_LAST_SENT_STATUS IS NOT NULL 
                            AND isnull(co.CAMPAIGN_SCHEDULTED_DATE,GETDATE()) <= GETDATE()
                            ORDER BY [start_date]";
                }

                if (!string.IsNullOrEmpty(sql))
                {
                    var dt = new DataTable();
                    using (SqlConnection connection = new SqlConnection(NewsLetterConnectionString))
                    {
                        connection.Open();
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
                            newRow["t"] = dr["t"];
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
        /// Count post 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Count([FromBody] Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            DataSet campaign = Campaign(dic);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "select create_date , count from[count] where dept = @dept  and page = @page and convert(char(8),create_date,112)=@date";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] == "HQ" ? "NULL" : dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@date", Value = DateTime.UtcNow.Date });

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                sqlDataAdapter.Fill(ds);
                connection.Close();
            }
            /// 如果有資料
            if (ds.Tables[0].Rows.Count > 0)
            {
                CountUpdate(dic, Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[1]));
            }
            /// 如果沒資料
            else
            {
                CountInst(dic);
            }

            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }
        /// <summary>
        /// 單元內容維護 更新
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public int CountUpdate([FromBody] Dictionary<string, string> dic, int Count)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "Update [count] set [count] = @count where dept =@dept  and page =@page and convert(char(8),create_date,112)=@date";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@count", Value = Count += 1 });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] == "HQ" ? "NULL" : dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@date", Value = DateTime.UtcNow.Date });

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
        public int CountInst([FromBody] Dictionary<string, string> dic)
        {
            int iCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "insert into [count] (count,page,dept) values (1,@page ,@dept )";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dept", Value = dic["dept"] == "HQ" ? "NULL" : dic["dept"] });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@page", Value = dic["page"] });

                iCount = command.ExecuteNonQuery();
                connection.Close();
            }
            return iCount;
        }


        /// <summary>
        /// banner 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage BannerData([FromBody] Dictionary<string, string> dic)
        {
            DataSet ds = new DataSet();
            //奕丞 2022/4/18 加入 cache 機制
            ObjectCache cache = MemoryCache.Default;
            string cacheKey = "BannerData";
            var objects = cache[cacheKey];
            if (objects != null && objects is DataSet set && 1==0)
            {
                ds = set;
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "Select top 5 descpt, urlpath, background from News where convert(varchar(8),getdate(),112)" +
                                 "between convert(varchar(8),start_date,112) and convert(varchar(8),end_date,112) and type = 'd_title' and isnull(dept,'')= ''" +
                                 "order by priority, create_date desc ";
                    SqlCommand command = new SqlCommand(sql, connection);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }
                DateTime expiredTime = DateTime.Now.Add(new TimeSpan(12, 0, 0));
                DateTimeOffset timeOffset = new DateTimeOffset(expiredTime);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = timeOffset
                };
                if (cache.Contains(cacheKey))
                    cache.Remove(cacheKey);
                cache.Add(cacheKey, ds, policy);
            }
            return Request.CreateResponse(HttpStatusCode.OK, ds);
        }

        [AcceptVerbs("Get", "Post")]
        public IHttpActionResult ReleaseAllCache()
        {
            ObjectCache cache = MemoryCache.Default;
            var cc = (from item in cache select item.Key).ToList();
            foreach (var c in cc)
                cache.Remove(c);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }
    }
}
