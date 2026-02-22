using HQBackSite.Controllers;
using HQBackSite.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HQBackSite.Tests
{
    [TestClass]
    public class MenuControllerIcsTests
    {
        [TestMethod]
        public void ICS_Get_ShouldReturnViewWithNewsModel()
        {
            var controller = new TestableMenuController();

            var result = controller.ICS() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(NewsModel));
        }

        [TestMethod]
        public void ICSAdd_Get_ShouldReturnViewWithNewsModel()
        {
            var controller = new TestableMenuController();

            var result = controller.ICSAdd() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(NewsModel));
        }

        [TestMethod]
        public void ICSQuery_Get_ShouldReturnIcsQueryPartialView()
        {
            var controller = new TestableMenuController();
            controller.QueryResultsByType[typeof(NewsModel)] = new List<NewsModel>
            {
                new NewsModel { des_no = 1, TotalCount = 2 }
            };

            var result = controller.ICSQuery(new NewsModel { PageNo = 1, PageSize = 10 }) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ICSQuery", result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(PagedListModel<NewsModel>));
        }

        [TestMethod]
        public void ICSAdd_Post_WhenDeptEmpty_ShouldFail()
        {
            var controller = new TestableMenuController();

            var result = controller.ICSAdd(new NewsModel { dept = "" });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "請選擇訊息類別");
        }

        [TestMethod]
        public void ICSAdd_Post_SecurityDept_ShouldInsertNewsAndIcsList()
        {
            var controller = new TestableMenuController();
            controller.Account = "A1234567";
            controller.QuerySingleResults.Enqueue(99);
            controller.FormValues["icsSecurityData"] = "[{\"Category\":\"資安\",\"Content\":\"測試文件\",\"DocUrl\":\"https://a\"}]";

            var result = controller.ICSAdd(new NewsModel
            {
                dept = "資安文件",
                background = "標題",
                priority = 1,
                start_date = DateTime.Today,
                end_date = DateTime.Today.AddDays(1)
            });

            var content = GetContent(result);
            StringAssert.Contains(content.Content, "\"code\":1");
            Assert.AreEqual(1, controller.QuerySingleCalls.Count, "應先寫入 news 並回傳 des_no");
            Assert.AreEqual(1, controller.ExecuteCalls.Count, "應新增一筆 ics_list");
        }

        [TestMethod]
        public void ICSUpdate_Get_ShouldLoadNewsAndIcsGroup()
        {
            var controller = new TestableMenuController();
            controller.QuerySingleResults.Enqueue(new NewsModel { des_no = 5, dept = "ISO文件" });
            controller.QueryResultsByType[typeof(IcsListModel)] = new List<IcsListModel> { new IcsListModel { ListID = 1 } };
            controller.QueryResultsByType[typeof(IcsUploadModel)] = new List<IcsUploadModel> { new IcsUploadModel { UpdateID = 2 } };

            var result = controller.ICSUpdate(5) as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as NewsModel;
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.IcsGroup);
            Assert.AreEqual(1, model.IcsGroup.list.Count);
            Assert.AreEqual(1, model.IcsGroup.uploads.Count);
        }

        [TestMethod]
        public void ICSUpdate_Post_WhenDeptEmpty_ShouldFail()
        {
            var controller = new TestableMenuController();

            var result = controller.ICSUpdate(new NewsModel { des_no = 1, dept = "" });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "請選擇訊息類別");
        }

        [TestMethod]
        public void ICSUpdate_Post_OtherDept_ShouldMarkAllIcsListDeleted()
        {
            var controller = new TestableMenuController();

            var result = controller.ICSUpdate(new NewsModel
            {
                des_no = 10,
                dept = "上傳文件",
                background = "B",
                priority = 1,
                start_date = DateTime.Today,
                end_date = DateTime.Today.AddDays(1)
            });

            var content = GetContent(result);
            StringAssert.Contains(content.Content, "\"code\":1");
            Assert.AreEqual(2, controller.ExecuteCalls.Count, "應包含 news 更新 + ics_list 全部刪除");
        }

        [TestMethod]
        public void ICSUpdate_Post_IsoDept_ShouldExecuteDeleteThenUpdateAndInsert()
        {
            var controller = new TestableMenuController();
            controller.Account = "A1234567";
            controller.FormValues["icsIsoData"] =
                "[" +
                "{\"ListID\":5,\"Deleted\":0,\"SerialNumber\":\"1\",\"DocNo\":\"D-1\",\"DocName\":\"N1\",\"DocUrl\":\"U1\",\"AttachmentInfo\":\"A1\",\"MainUser\":\"M1\",\"Remark\":\"R1\"}," +
                "{\"ListID\":0,\"Deleted\":0,\"SerialNumber\":\"2\",\"DocNo\":\"D-2\",\"DocName\":\"N2\",\"DocUrl\":\"U2\",\"AttachmentInfo\":\"A2\",\"MainUser\":\"M2\",\"Remark\":\"R2\"}" +
                "]";

            var result = controller.ICSUpdate(new NewsModel
            {
                des_no = 20,
                dept = "ISO文件",
                background = "B",
                priority = 1,
                start_date = DateTime.Today,
                end_date = DateTime.Today.AddDays(1)
            });

            var content = GetContent(result);
            StringAssert.Contains(content.Content, "\"code\":1");
            Assert.AreEqual(4, controller.ExecuteCalls.Count, "應包含 news 更新 + 全刪 + 更新舊資料 + 新增資料");
        }

        private static ContentResult GetContent(ActionResult result)
        {
            var content = result as ContentResult;
            Assert.IsNotNull(content, "應回傳 ContentResult(JSON)");
            return content;
        }

        private class TestableMenuController : MenuController
        {
            internal string Account { get; set; } = "TESTER";
            internal Dictionary<string, string> FormValues { get; } = new Dictionary<string, string>();
            internal Queue<object> QuerySingleResults { get; } = new Queue<object>();
            internal Dictionary<Type, object> QueryResultsByType { get; } = new Dictionary<Type, object>();
            internal List<string> QuerySingleCalls { get; } = new List<string>();
            internal List<string> ExecuteCalls { get; } = new List<string>();

            protected override string GetCurrentAccount()
            {
                return Account;
            }

            protected override string GetFormValue(string key)
            {
                return FormValues.ContainsKey(key) ? FormValues[key] : null;
            }

            protected override T QuerySingleInternal<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
            {
                QuerySingleCalls.Add(sql);

                if (QuerySingleResults.Count == 0)
                {
                    return default(T);
                }

                var next = QuerySingleResults.Dequeue();
                return (T)next;
            }

            protected override List<T> QueryInternal<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
            {
                if (QueryResultsByType.TryGetValue(typeof(T), out var value))
                {
                    return value as List<T> ?? new List<T>();
                }

                return new List<T>();
            }

            protected override int ExecuteInternal(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
            {
                ExecuteCalls.Add(sql);
                return 1;
            }
        }
    }
}