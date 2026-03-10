using HQBackSite.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HQBackSite.Tests
{
    [TestClass]
    public class MenuControllerWordHtmlTests
    {
        [TestMethod]
        public void WordHtmlTest_Get_ShouldReturnView()
        {
            var controller = CreateControllerWithContext(new EmptyFileCollection());

            var result = controller.WordHtmlTest() as ViewResult;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void WordHtmlConvert_Post_WhenNoFile_ShouldFail()
        {
            var controller = CreateControllerWithContext(new EmptyFileCollection());

            var result = controller.WordHtmlConvert();
            var content = GetContentResult(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "請先選擇要上傳的 Word 檔案");
        }

        [TestMethod]
        public void WordHtmlConvert_Post_WhenInvalidExtension_ShouldFail()
        {
            var files = new SingleFileCollection(new FakePostedFile("bad.txt", 1024));
            var controller = CreateControllerWithContext(files);

            var result = controller.WordHtmlConvert();
            var content = GetContentResult(result);

            StringAssert.Contains(content.Content, "\"code\":0");
            StringAssert.Contains(content.Content, "目前僅支援 .docx 檔案");
        }

        [TestMethod]
        public void WordHtmlConvert_Post_WhenValidDocx_ShouldReturnSuccess()
        {
            var files = new SingleFileCollection(new FakePostedFile("sample.docx", 2048));
            var controller = CreateControllerWithContext(files);
            controller.ConvertResultHtml = "<!DOCTYPE html><html><body><p>ok</p></body></html>";

            var result = controller.WordHtmlConvert();
            var content = GetContentResult(result);

            StringAssert.Contains(content.Content, "\"code\":1");
            StringAssert.Contains(content.Content, "\"previewUrl\"");
            Assert.AreEqual(1, controller.SavedFiles.Count);
            Assert.AreEqual(1, controller.WrittenFiles.Count);
            Assert.IsTrue(controller.ForcePdfHyperlinksReceived);
        }

        [TestMethod]
        public void WordHtmlPreview_Get_WhenInvalidFilename_ShouldReturnNotFound()
        {
            var controller = CreateControllerWithContext(new EmptyFileCollection());

            var result = controller.WordHtmlPreview("../unsafe.html");

            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
        }

        [TestMethod]
        public void WordHtmlPreview_Get_WhenFileExists_ShouldReturnHtml()
        {
            var controller = CreateControllerWithContext(new EmptyFileCollection());
            var outputDir = Path.Combine(controller.TestRoot, "Views", "GeneratedHtml");
            Directory.CreateDirectory(outputDir);
            File.WriteAllText(Path.Combine(outputDir, "test.html"), "<html><body>preview</body></html>");

            var result = controller.WordHtmlPreview("test.html");
            var content = result as ContentResult;

            Assert.IsNotNull(content);
            StringAssert.Contains(content.Content, "preview");
            Assert.AreEqual("text/html", content.ContentType);
        }

        private static TestableMenuController CreateControllerWithContext(HttpFileCollectionBase files)
        {
            var controller = new TestableMenuController();
            var request = new FakeHttpRequest(files);
            var httpContext = new FakeHttpContext(request);
            var routeData = new RouteData();
            routeData.Values["controller"] = "Menu";
            routeData.Values["action"] = "WordHtmlConvert";

            controller.ControllerContext = new ControllerContext(httpContext, routeData, controller);

            var routes = new RouteCollection();
            routes.MapRoute("Default", "{controller}/{action}/{id}", new { id = UrlParameter.Optional });
            controller.Url = new UrlHelper(new RequestContext(httpContext, routeData), routes);

            return controller;
        }

        private static ContentResult GetContentResult(ActionResult result)
        {
            var content = result as ContentResult;
            Assert.IsNotNull(content);
            return content;
        }

        private class TestableMenuController : MenuController
        {
            internal string ConvertResultHtml { get; set; } = "<html><body>default</body></html>";
            internal List<string> SavedFiles { get; } = new List<string>();
            internal List<string> WrittenFiles { get; } = new List<string>();
            internal string TestRoot { get; } = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlTests", Guid.NewGuid().ToString("N"));
            internal bool ForcePdfHyperlinksReceived { get; private set; }

            protected override string MapPathInternal(string virtualPath)
            {
                var relative = (virtualPath ?? string.Empty).TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
                return Path.Combine(TestRoot, relative);
            }

            protected override void SavePostedFileInternal(HttpPostedFileBase file, string fullPath)
            {
                SavedFiles.Add(fullPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                System.IO.File.WriteAllText(fullPath, "fake-docx");
            }

            protected override string ConvertWordToHtmlInternal(string docxPath, string imageOutputDirectory, string imageUrlPrefix, bool forcePdfHyperlinks)
            {
                ForcePdfHyperlinksReceived = forcePdfHyperlinks;
                return ConvertResultHtml;
            }

            protected override void WriteFileTextInternal(string fullPath, string content)
            {
                WrittenFiles.Add(fullPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                System.IO.File.WriteAllText(fullPath, content);
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
            public override Uri Url => new Uri("http://localhost/Menu/WordHtmlConvert");
            public override string ApplicationPath => "/";
        }

        private class EmptyFileCollection : HttpFileCollectionBase
        {
            public override int Count => 0;
        }

        private class SingleFileCollection : HttpFileCollectionBase
        {
            private readonly HttpPostedFileBase _file;

            public SingleFileCollection(HttpPostedFileBase file)
            {
                _file = file;
            }

            public override int Count => 1;
            public override HttpPostedFileBase this[int index] => index == 0 ? _file : null;
        }

        private class FakePostedFile : HttpPostedFileBase
        {
            private readonly string _fileName;
            private readonly int _contentLength;

            public FakePostedFile(string fileName, int contentLength)
            {
                _fileName = fileName;
                _contentLength = contentLength;
            }

            public override string FileName => _fileName;
            public override int ContentLength => _contentLength;
        }
    }
}
