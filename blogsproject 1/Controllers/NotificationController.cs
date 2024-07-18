using blogsproject_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace blogsproject_1.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    [Authorize(Policy = "AdminPolicy")]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Nofications
                .Include(n => n.AdminUser)
                .OrderByDescending(n => n.SentDate)
                .ToListAsync();

            return View(notifications);
        }
    }
}