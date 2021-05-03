using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationProject.Data;
using RegistrationProject.Services.DiscordBot;

namespace RegistrationProject.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class MemberController : Controller
    {
        private readonly BotConfig _botConfig;

        public MemberController(BotConfig botConfig = null)
        {

            _botConfig = botConfig;
        }
        public IActionResult Approve([FromQuery] int id)
        {
            using MemberDbContext db = new(_botConfig.ConnectionString);
            db.Database.EnsureCreated();
            db.Members.Find(id).IsApproved = true;
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Remove([FromQuery] int id)
        {
            using MemberDbContext db = new(_botConfig.ConnectionString);
            db.Database.EnsureCreated();
            db.Remove(db.Members.Find(id));
            db.SaveChanges();
            return RedirectToAction("Index", "Home");


        }
    }
}
