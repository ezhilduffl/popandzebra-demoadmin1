using Microsoft.AspNetCore.Mvc;

namespace PopZebra.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error")]
        public IActionResult Index()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}