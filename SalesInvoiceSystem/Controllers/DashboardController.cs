using Microsoft.AspNetCore.Mvc;

namespace SalesInvoiceSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
