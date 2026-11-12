using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
using var directoryFiles = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "DirectoryFiles"));
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/Areas/tel",
    FileProvider = directoryFiles,
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".htm"] = "text/html; charset=big5",
        [".css"] = "text/css; charset=big5",
        [".xml"] = "application/xml"
    })
});
app.MapControllers();
app.Run();
