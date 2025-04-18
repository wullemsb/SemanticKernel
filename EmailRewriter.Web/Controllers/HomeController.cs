using EmailRewriter.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EmailRewriter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "emailexample.txt");
            var emailContent = System.IO.File.ReadAllText(filePath);
            
            return View(new EmailContentModel { Content = emailContent });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
