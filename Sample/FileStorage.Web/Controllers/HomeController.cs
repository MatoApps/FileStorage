using Microsoft.AspNetCore.Mvc;

namespace FileStorage.Web.Controllers
{
    public class HomeController : FileStorageControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}