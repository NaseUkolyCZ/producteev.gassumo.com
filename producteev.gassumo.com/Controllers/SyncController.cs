using naseukolycz.universalsync.communicator.exchange;
using naseukolycz.universalsync.communicator.producteev;
using naseukolycz.universalsync.icommunicator;
using naseukolycz.universalsync.idata;
using naseukolycz.universalsync.nuefdatamodel;
using naseukolycz.universalsync.nuefdatamodel.Repositories;
using Oak.Controllers;
using producteev.gassumo.com.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace producteev.gassumo.com.Controllers
{
    public class SyncController : BaseController
    {
        private Accounts accounts = new Accounts();
        private ProducteevTasks producteevTasks = new ProducteevTasks();

        [Authorize]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Login");
            }

            SetAccountInformationViewBag();
            return View("Index");
        }

        [HttpPost]
        [Authorize]
        public dynamic Sync(SyncPageModel model)
        {
            SeedController seed = new SeedController();

            if (!User.Identity.IsAuthenticated)
            {
                return View("Login");
            }

            List<naseukolycz.universalsync.idata.Account> accounts = GetAccountsFromUser();
            User u = (naseukolycz.universalsync.idata.User)Session["User"];
            foreach (var item in accounts)
            {
                if (item.Provider == naseukolycz.universalsync.idata.AccountProvider.Producteev && item.BelongsToUser == u.InternalId)
                {
                    item.UpdatesPolicy[typeof(NuTask)] = AccountUpdatesPolicy.DuplexUpdates;
                    if ((item.LastSync == null || item.LastSync.Count == 0) && item.BelongsToUser == u.InternalId)
                    {
                        FixAccountLastSync(item);
                    }
                }

                if (item.Provider == naseukolycz.universalsync.idata.AccountProvider.Exchange && item.BelongsToUser == u.InternalId)
                {
                    item.Username = model.ExchangeUsername;
                    item.Password = model.ExchangePassword;
                    item.UpdatesPolicy[typeof(NuTask)] = AccountUpdatesPolicy.DuplexUpdates;
                    if ((item.LastSync == null || item.LastSync.Count == 0) && item.BelongsToUser == u.InternalId)
                    {
                        FixAccountLastSync(item);
                    }
                }
            }

            foreach (var item in accounts)
            {
                sql.Update(item.InternalId, item);
            }

            SetAccountInformationViewBag();
            ViewBag.SaveToDBSuccess = "Save to db is complete!";

            return View("Index");
        }

        [Authorize]
        public ActionResult Token(string code)
        {
            string token = producteevTasks.GetToken(code);
            SetUserToken((int)Session["UserId"], token);
            Session["ProducteevLogged"] = true;
            SetAccountInformationViewBag();
            return View("Index");
        }

        [Authorize]
        public void SetUserToken(int userId, string token)
        {
            var account = accounts.SingleWhere("UserId=@0 AND AccountProviderId=@1", args: new object[] { userId, naseukolycz.universalsync.idata.AccountProvider.Producteev });
            account.Password = token;
            accounts.Save(account);
        }

        [Authorize]
        public ActionResult Synchronize()
        {
            MasterSynchronizer.Start();
            SetAccountInformationViewBag();
            return View("Index");
        }

        [Authorize]
        public ActionResult DeleteDb()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.Identity.Name == "admin")
                {
                    SeedController seedController = new SeedController();
                    seedController.PurgeDb();
                    seedController.All();
                    seedController.SampleEntries();
                    return RedirectToAction("Logout", "User");
                }
                else
                {
                    ViewBag.NotDeletedDb = "You don't have permission for delete database!";
                    SetAccountInformationViewBag();
                }
            }
            else
            {
                ViewBag.NotDeletedDb = "You have to be logged for delete database!";
            }

            return View("Index");
        }

        internal DateTime fix(DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
        }

        internal void FixAccountLastSync(Account iAccount)
        {
            iAccount.LastSync = new Dictionary<Type, DateTime>();
            iAccount.LastSync[typeof(NuTask)] = fix(DateTime.Now.AddMonths(-2));
        }

    }
}