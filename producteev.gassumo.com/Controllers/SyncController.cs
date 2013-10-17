using atollon.sync.communicator.producteev;
using atollon.sync.idata;
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
        private ProducteevTasks producteevTasks  = new ProducteevTasks();
        Accounts accounts = new Accounts();

        public void SetUserToken(int userId, string token)
        {
            var account = accounts.SingleWhere("UserId=@0 AND AccountProviderId=@1", args: new object[] { userId, AccountProvider.Producteev });
            account.Password = token;
            accounts.Save(account);
        }

        public ActionResult Index()
        {
            var user = User();
            ViewBag.User = user;
            ViewBag.SetupProducteevToken = producteevTasks.GetAuthenticateUrl();

            return View();
        }
        public ActionResult Token(string code)
        {
            var user = User();
            ViewBag.User = user;
            string token = producteevTasks.GetToken(code);
            SetUserToken(user.Id, token);

            return View("Index");
        }
    }
}