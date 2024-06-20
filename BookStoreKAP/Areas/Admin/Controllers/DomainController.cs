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
    [Area(AreasConstant.ADMIN), Authorize(Roles = RolesConstant.ADMIN)]
    public class DomainController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private readonly RoleManager<Role> _roleManager;

        public DomainController(BookStoreKAPDBContext context, RoleManager<Role> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public IActionResult Index(Guid roleID)
        {
            var domainList = _context.Domains.Where(x => x.RoleID == roleID).ToList();
            return View(domainList);
        }

        public IActionResult GetAllDomainsByAccessControllerID(Guid accessControllerID)
        {
            var domains = _context.Domains.Include(x => x.AccessController).Include(x => x.Role).Where(x => x.AccessControllerID == accessControllerID).ToList();

            ViewBag.AccessControllerID = accessControllerID;

            return View(domains);
        }

        public IActionResult Create(Guid accessControllerID)
        {
            var roles = _context.Roles.ToList();

            ViewBag.AccessControllerID = accessControllerID;

            return View(roles);
        }

        [HttpPost]
        public IActionResult Create(ReqCreateDomain req)
        {
            var domain = new Domain() { RoleID = req.RoleID, AccessControllerID = req.AccessControllerID };
            _context.Domains.Add(domain);
            _context.SaveChanges();

            return RedirectToAction("GetAllDomainsByAccessControllerID", new { accessControllerID = req.AccessControllerID });
        }
    }
}
