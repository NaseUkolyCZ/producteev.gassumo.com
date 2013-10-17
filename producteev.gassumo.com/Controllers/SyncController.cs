using atollon.sync.nuefdatamodel.Repositories;
using Oak.Controllers;
using PiranhaCMSOak.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace producteev.gassumo.com.Controllers
{
    [Authorize]
    [MyRequireHttps]
    public class SyncController : PiranhaCMSOak.Controllers.BaseController
    {
        public ActionResult Index()
        {
            var user = User();
            ViewBag.User = user;

            return View();
        }
    }
}