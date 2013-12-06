using naseukolycz.universalsync.communicator.exchange;
using naseukolycz.universalsync.communicator.producteev;
using naseukolycz.universalsync.icommunicator;
using naseukolycz.universalsync.nuefdatamodel;
using naseukolycz.universalsync.nuefdatamodel.Models;
using naseukolycz.universalsync.nuefdatamodel.Repositories;
using Oak.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace producteev.gassumo.com.Controllers
{
    public class UserController : BaseController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ProducteevTasks producteevTasks = new ProducteevTasks();
        private LoginForWebs l4ws = new LoginForWebs();
        private Users users = new Users();
        public Action<string> Authenticate { get; set; }

        public UserController()
        {
            Authenticate = username => FormsAuthentication.RedirectFromLoginPage(username, false);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Sync");
            }

            return View("Register");
        }

        [HttpPost]
        [AllowAnonymous]
        public dynamic Register(LoginForWeb model)
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Sync");
            }

            string isValidRegister = model.IsValidRegister(model);

            if (isValidRegister != "")
            {
                ViewBag.Flash = isValidRegister;

                return View("Register");
            }

            if (model == null)
            {
                ViewBag.Flash = "Your account wasn't created!";
                return View("Register");
            }

            Users users = new Users();
            naseukolycz.universalsync.idata.User iUser = new naseukolycz.universalsync.idata.User();
            iUser.IsBlocked = false;
            iUser.Created = DateTime.Now.AddMonths(-1);
            iUser.InternalId = Guid.NewGuid();
            // http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
            iUser.Created = iUser.Created.AddTicks(-(iUser.Created.Ticks % TimeSpan.TicksPerSecond));
            naseukolycz.universalsync.idata.User createdUser = sql.CreateUser(iUser);

            if (createdUser == null)
            {
                log.ErrorFormat("Error with creating User in database!");
                ViewBag.Flash = "Error with creating User in database!";
                return View("Register");
            }

            model.UserId = createdUser.InternalId.ToString();
            dynamic result = l4ws.Register(model);

            if (result == null)
            {
                log.ErrorFormat("User wasn't create!");
                ViewBag.Flash = "User wasn't create!";
                return View("Register");
            }

            return View("Login");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated && Session["User"] != null)
            {
                return RedirectToAction("Index", "Sync");
            }

            if (Session["User"] == null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "User");
            }

            return View("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        public dynamic Login(LoginForWeb model)
        {
            string isValidLogin = model.IsValidLogin();
            if (isValidLogin != "")
            {
                ViewBag.Flash = isValidLogin;
                return View("Login");
            }

            dynamic result = l4ws.Login(model.Username, model.Password);

            if (result == null)
            {
                ViewBag.Flash = "Username or password was entered wrong!";
                return View("Login");
            }

            Guid guid = new Guid(result.UserId);
            Session["User"] = sql.GetUser(guid);
            Session["UserId"] = users.SingleWhere("InternalId = @0", result.UserId).Id;

            //create accounts when dont exists
            if (GetAccountsFromUser().Count() <= 0)
            {
                naseukolycz.universalsync.idata.Account producteev = new naseukolycz.universalsync.idata.Account();
                producteev.InternalId = Guid.NewGuid();

                naseukolycz.universalsync.idata.User u = (naseukolycz.universalsync.idata.User)Session["User"];
                producteev.BelongsToUser = u.InternalId;

                producteev.Provider = naseukolycz.universalsync.idata.AccountProvider.Producteev;

                sql.CreateAccount(producteev);

                naseukolycz.universalsync.idata.Account exchange = new naseukolycz.universalsync.idata.Account();
                exchange.InternalId = Guid.NewGuid();
                exchange.BelongsToUser = u.InternalId;
                exchange.Provider = naseukolycz.universalsync.idata.AccountProvider.Exchange;

                sql.CreateAccount(exchange);
            }

            SetAccountInformationViewBag();

            //FormsAuthentication.SetAuthCookie(model.Username, true);

            Authenticate(model.Username);

            return RedirectToAction("Index", "Sync");
        }

        [Authorize]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();                
            return View("Login");
        }

    }
}

