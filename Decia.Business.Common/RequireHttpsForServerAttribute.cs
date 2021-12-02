using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Decia.Business.Common
{
    public class RequireHttpsForServerAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            { throw new ArgumentNullException("The \"filterContext\" must not be null."); }

            var isLocal = filterContext.HttpContext.Request.IsLocal;
            if (isLocal)
            { return; }

            base.OnAuthorization(filterContext);
        }
    }
}