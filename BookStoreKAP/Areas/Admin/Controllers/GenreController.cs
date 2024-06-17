﻿using BookStoreKAP.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    [Authorize(Roles = RolesConstant.ADMIN)]
    public class GenreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Modify(Guid genreID)
        {
            return View();
        }
    }
}
