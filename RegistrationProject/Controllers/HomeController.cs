using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationProject.Data;
using RegistrationProject.Models;
using RegistrationProject.Services.DiscordBot;
using System.Diagnostics;
using System.Linq;


namespace RegistrationProject.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class HomeController : Controller
    {

        private readonly BotConfig _botConfig;

        public HomeController(BotConfig botConfig = null)
        {

            _botConfig = botConfig;
        }

        public IActionResult Index()
        {
            using MemberDbContext db = new(_botConfig.ConnectionString);
            db.Database.EnsureCreated();


            return View(db.Members.ToList());
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
