using System.Web;
using System.Web.Optimization;

namespace HQ
{
    public class BundleConfig
    {
        // 如需統合的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/util").Include(
                  "~/Scripts/util.js"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好可進行生產時，請使用 https://modernizr.com 的建置工具，只挑選您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //奕丞 2022/2/25 將常用 js 檔案壓縮到 bundle table 當中
            var layoutJs = new ScriptBundle("~/Scripts/layout");
            layoutJs
                .Include("~/Scripts/Hq/Left_btn.js")
                .Include("~/Scripts/Home/mm_menu.js");

            var deptJs= new ScriptBundle("~/Scripts/dept");
            deptJs.Include("~/Scripts/Hq/Dept.js");

            bundles.Add(layoutJs);
            bundles.Add(deptJs);

#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
