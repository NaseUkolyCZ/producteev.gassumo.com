using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace producteev.gassumo.com.Controllers
{
    public class UserController : PiranhaCMSOak.Controllers.UserController
    {
        protected override Guid GetUserGroupId()
        {
            return new Guid("8940b41a-e3a9-44f3-b564-bfd281416142");
        }
    }
}
