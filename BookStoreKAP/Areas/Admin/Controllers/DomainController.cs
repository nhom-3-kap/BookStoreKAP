using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class DomainController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private readonly RoleManager<Role> _roleManager;

        public DomainController(BookStoreKAPDBContext context, RoleManager<Role> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        [Authorize(Policy = "CanView")]
        public IActionResult Index(Guid roleID, string roleName)
        {
            var domainList = _context.Domains.Include(x => x.AccessController).Include(x => x.Role).Where(x => x.RoleID == roleID).ToList();
            ViewBag.RoleName = roleName;
            return View(domainList);
        }

        [Authorize(Policy = "CanView")]
        public IActionResult DomainsByAccessController(Guid accessControllerID)
        {
            var domains = _context.Domains.Include(x => x.AccessController).Include(x => x.Role).Where(x => x.AccessControllerID == accessControllerID).ToList();

            ViewBag.AccessControllerID = accessControllerID;

            return View(domains);
        }

        [Authorize(Policy = "CanCreate")]
        public IActionResult Create(Guid accessControllerID)
        {
            var roles = _context.Roles.ToList();

            ViewBag.AccessControllerID = accessControllerID;

            return View(roles);
        }

        [Authorize(Policy = "CanCreate")]
        [HttpPost]
        public IActionResult Create(ReqCreateDomain req)
        {
            var domain = new Domain() { RoleID = req.RoleID, AccessControllerID = req.AccessControllerID };
            _context.Domains.Add(domain);
            _context.SaveChanges();

            return RedirectToAction("DomainsByAccessController", new { accessControllerID = req.AccessControllerID });
        }
    }
}
