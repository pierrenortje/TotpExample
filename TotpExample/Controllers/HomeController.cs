using Microsoft.AspNetCore.Mvc;

namespace TotpExample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
