using Microsoft.AspNetCore.Mvc;

namespace HQ.Portal.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/Home/FaxView")]
    [HttpHead("/Home/FaxView")]
    public IActionResult FaxView() => View();

    [HttpGet("/Home/Restaurant")]
    [HttpHead("/Home/Restaurant")]
    public IActionResult Restaurant() => View();
}
