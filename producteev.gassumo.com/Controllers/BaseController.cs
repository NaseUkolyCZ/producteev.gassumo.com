using naseukolycz.universalsync.communicator.exchange;
using naseukolycz.universalsync.communicator.producteev;
using naseukolycz.universalsync.idata;
using naseukolycz.universalsync.nuefdatamodel;
using Oak.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace producteev.gassumo.com.Controllers
{
    public class BaseController : Controller
    {
        public SQLDataModel sql = new SQLDataModel();
        private ProducteevTasks producteevTasks = new ProducteevTasks();

        public BaseController()
        {
        }

        public List<naseukolycz.universalsync.idata.Account> GetAccountsFromUser()
        {
            List<naseukolycz.universalsync.idata.Account> list = new List<naseukolycz.universalsync.idata.Account>();

            User u = (naseukolycz.universalsync.idata.User)Session["User"];
            Dictionary<Guid, naseukolycz.universalsync.idata.Account> dic = sql.GetAccounts(u.InternalId);

            foreach (KeyValuePair<Guid, naseukolycz.universalsync.idata.Account> entry in dic)
            {
                list.Add(entry.Value);
            }

            return list;
        }

        public void SetAccountInformationViewBag()
        {
            List<naseukolycz.universalsync.idata.Account> accounts = GetAccountsFromUser();

            naseukolycz.universalsync.idata.Account producteev = new naseukolycz.universalsync.idata.Account();
            naseukolycz.universalsync.idata.Account exchange = new naseukolycz.universalsync.idata.Account();
            
            foreach (var item in accounts)
            {
                if (item.Provider == naseukolycz.universalsync.idata.AccountProvider.Producteev)
                {
                    producteev = item;
                }

                if (item.Provider == naseukolycz.universalsync.idata.AccountProvider.Exchange)
                {
                    exchange = item;
                }
            }

            ViewBag.ExchangeUsername = exchange.Username;
            ViewBag.ExchangePassword = exchange.Password;
            ViewBag.ProducteevToken = producteev.Password;
            ViewBag.SetupProducteevToken = producteevTasks.GetAuthenticateUrl();

            ViewBag.Info = "This information will be use for synchronize!";
        }

    }
}
