using atollon.sync.icommunicator;
using atollon.sync.idata;
using atollon.sync.nuefdatamodel;
using atollon.sync.nuefdatamodel.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace producteev.gassumo.com.Controllers
{
    public class UserController : PiranhaCMSOak.Controllers.UserController
    {
        protected new Users users;

        public UserController()
        {
            users = new Users();
        }

        internal DateTime fix(DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
        }

        protected override Guid GetUserGroupId()
        {
            return new Guid("8940b41a-e3a9-44f3-b564-bfd281416142");
        }

        protected override void InsertAdditionalRegistrationProperties(dynamic UserRegistration)
        {
            UserRegistration.InternalId = Guid.NewGuid();
            UserRegistration.Created = DateTime.Now;
            UserRegistration.IsBlocked = false;
        }

        protected override void PostUserInsert(int userId)
        {
            var user = users.Single(userId);
            Guid userInternalId = user.InternalId;
            GimmeExchangeAccount(userInternalId);
            GimmeProducteevAccount(userInternalId);
        }

        protected Account GimmeExchangeAccount(Guid userInternalId)
        {
            Account iAccount = new Account();
            iAccount.InternalId = Guid.NewGuid();
            iAccount.BelongsToUser = userInternalId;
            iAccount.Username = "joe@outlook.com";
            iAccount.Password = "MyPa$$w0rd123";
            iAccount.Server = string.Empty;
            iAccount.Provider = AccountProvider.Exchange;
            FixAccountDicts(iAccount);

            SQLDataModel sql = new SQLDataModel();
            Account result = sql.CreateAccount(iAccount);

            return result;
        }
        protected Account GimmeProducteevAccount(Guid userInternalId)
        {
            Account iAccount = new Account();
            iAccount.InternalId = Guid.NewGuid();
            iAccount.BelongsToUser = userInternalId;
            iAccount.Username = string.Empty;
            iAccount.Password = string.Empty;
            iAccount.Server = string.Empty;
            iAccount.Provider = AccountProvider.Producteev;
            FixAccountDicts(iAccount);

            SQLDataModel sql = new SQLDataModel();
            Account result = sql.CreateAccount(iAccount);

            return result;
        }

        protected void FixAccountDicts(Account iAccount)
        {
            iAccount.UpdatesPolicy = new Dictionary<Type, AccountUpdatesPolicy>();
            iAccount.LastSync = new Dictionary<Type, DateTime>();
            iAccount.UpdatesPolicy[typeof(NuContact)] = AccountUpdatesPolicy.DuplexUpdates;
            iAccount.LastSync[typeof(NuContact)] = fix(DateTime.Now.AddMonths(-1));
            iAccount.UpdatesPolicy[typeof(NuEvent)] = AccountUpdatesPolicy.DuplexUpdates;
            iAccount.LastSync[typeof(NuEvent)] = fix(DateTime.Now.AddMonths(-1));
            iAccount.UpdatesPolicy[typeof(NuTask)] = AccountUpdatesPolicy.DuplexUpdates;
            iAccount.LastSync[typeof(NuTask)] = fix(DateTime.Now.AddMonths(-1));
        }
    }
}
