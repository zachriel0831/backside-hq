using HQBackSite.Controllers;
using HQBackSite.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HQBackSite.Tests
{
    [TestClass]
    public class MenuControllerContentTests
    {
        [TestMethod]
        public void Content_Get_ShouldReturnViewWithModelAndParas()
        {
            var controller = new TestableMenuController();
            controller.UserCodeNames.Add("D001");
            controller.QueryResultsByType[typeof(ParaModel)] = new List<ParaModel> { new ParaModel { code_name = "D001" } };

            var result = controller.Content() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(ContentModel));
            Assert.IsNotNull(result.ViewData["Paras"]);
        }

        [TestMethod]
        public void ContentQuery_Get_ShouldReturnPartialView()
        {
            var controller = new TestableMenuController();
            controller.UserCodeNames.Add("D001");
            controller.QueryResultsByType[typeof(ContentModel)] = new List<ContentModel>
            {
                new ContentModel { subject_id = 1, subject = "標題", TotalCount = 1 }
            };

            var result = controller.ContentQuery(new ContentModel { PageNo = 1, PageSize = 10 }) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ContentQuery", result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(PagedListModel<ContentModel>));
        }

        [TestMethod]
        public void GetCategories_WhenDeptEmpty_ShouldReturnSuccessEmptyList()
        {
            var controller = new TestableMenuController();

            var result = controller.GetCategories("");
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":1");
            StringAssert.Contains(content.Content, "\"data\":[]");
        }

        [TestMethod]
        public void GetCategories_WhenDeptProvided_ShouldReturnList()
        {
            var controller = new TestableMenuController();
            controller.QueryResultsByType[typeof(ContentModel)] = new List<ContentModel>
            {
                new ContentModel { page = "10", subject = "分類A" }
            };

            var result = controller.GetCategories("D001");
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":1");
            StringAssert.Contains(content.Content, "分類A");
        }

        [TestMethod]
        public void ContentAdd_Get_ShouldReturnViewWithContentModel()
        {
            var controller = new TestableMenuController();
            controller.UserCodeNames.Add("D001");
            controller.QueryResultsByType[typeof(ParaModel)] = new List<ParaModel> { new ParaModel { code_name = "D001" } };

            var result = controller.ContentAdd() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(ContentModel));
            Assert.IsNotNull(result.ViewData["Paras"]);
        }

        [TestMethod]
        public void ContentAdd_Post_WhenDeptEmpty_ShouldFail()
        {
            var controller = new TestableMenuController();

            var result = controller.ContentAdd(new ContentModel { dept = "" });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "請設定部門");
        }

        [TestMethod]
        public void ContentAdd_Post_WhenSubtypeDwAndUrlEmpty_ShouldFail()
        {
            var controller = new TestableMenuController();

            var result = controller.ContentAdd(new ContentModel
            {
                dept = "D001",
                page = "10",
                subtype = "d_dw",
                subject = "T",
                is_show = "Y",
                url = ""
            });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "請設定URL連結");
        }

        [TestMethod]
        public void ContentAdd_Post_Valid_ShouldSuccessAndExecuteInsert()
        {
            var controller = new TestableMenuController();

            var result = controller.ContentAdd(new ContentModel
            {
                dept = "D001",
                page = "10",
                subtype = "d_main",
                subject = "標題",
                is_show = "Y",
                content = "內文"
            });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":1");
            Assert.AreEqual(1, controller.ExecuteCalls.Count);
        }

        [TestMethod]
        public void ContentUpdate_Get_ShouldLoadContentAndLists()
        {
            var controller = new TestableMenuController();
            controller.UserCodeNames.Add("D001");
            controller.QuerySingleResults.Enqueue(new ContentModel { subject_id = 9, dept = "D001", subject = "S" });
            controller.QueryResultsByType[typeof(ParaModel)] = new List<ParaModel> { new ParaModel { code_name = "D001" } };
            controller.QueryResultsByType[typeof(ContentModel)] = new List<ContentModel> { new ContentModel { page = "10", subject = "分類A" } };

            var result = controller.ContentUpdate(9) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(ContentModel));
            Assert.IsNotNull(result.ViewData["Paras"]);
            Assert.IsNotNull(result.ViewData["Categorys"]);
        }

        [TestMethod]
        public void ContentUpdate_Post_WhenPageEmpty_ShouldFail()
        {
            var controller = new TestableMenuController();

            var result = controller.ContentUpdate(new ContentModel { page = "", subtype = "d_main", subject = "S", is_show = "Y" });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "請設定類別");
        }

        [TestMethod]
        public void ContentUpdate_Post_Valid_ShouldSuccessAndExecuteUpdate()
        {
            var controller = new TestableMenuController();

            var result = controller.ContentUpdate(new ContentModel
            {
                subject_id = 3,
                page = "10",
                subtype = "d_main",
                subject = "S",
                is_show = "Y",
                content = "C"
            });
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":1");
            Assert.AreEqual(1, controller.ExecuteCalls.Count);
        }

        [TestMethod]
        public void UploadFileForUrl_Post_WhenNoFile_ShouldFail()
        {
            var controller = new TestableMenuController();
            controller.ControllerContext = new ControllerContext(
                new FakeHttpContext(new FakeHttpRequest(new EmptyFileCollection())),
                new RouteData(),
                controller);

            var result = controller.UploadFileForUrl();
            var content = GetContent(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "沒有上傳文件");
        }

        private static ContentResult GetContent(ActionResult result)
        {
            var content = result as ContentResult;
            Assert.IsNotNull(content, "應回傳 ContentResult(JSON)");
            return content;
        }

        private class TestableMenuController : MenuController
        {
            internal List<string> UserCodeNames { get; } = new List<string>();
            internal Queue<object> QuerySingleResults { get; } = new Queue<object>();
            internal Dictionary<Type, object> QueryResultsByType { get; } = new Dictionary<Type, object>();
            internal List<string> ExecuteCalls { get; } = new List<string>();

            protected override List<string> GetUserCodeNamesInternal()
            {
                return UserCodeNames;
            }

            protected override T QuerySingleInternal<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
            {
                if (QuerySingleResults.Count == 0)
                {
                    return default(T);
                }

                return (T)QuerySingleResults.Dequeue();
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

        private class FakeHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;
            public FakeHttpContext(HttpRequestBase request)
            {
                _request = request;
            }
            public override HttpRequestBase Request => _request;
        }

        private class FakeHttpRequest : HttpRequestBase
        {
            private readonly HttpFileCollectionBase _files;
            public FakeHttpRequest(HttpFileCollectionBase files)
            {
                _files = files;
            }
            public override HttpFileCollectionBase Files => _files;
            public override Uri Url => new Uri("http://localhost/Menu/UploadFileForUrl");
        }

        private class EmptyFileCollection : HttpFileCollectionBase
        {
            public override int Count => 0;
        }
    }
}