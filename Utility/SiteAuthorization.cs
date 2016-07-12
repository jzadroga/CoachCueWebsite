using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;

namespace CoachCue.Utility
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class SiteAuthorizationAttribute : AuthorizeAttribute
    {
        public string COACHCUE_AUTH_COOKIE = "coachcue_auth";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            if (IsAuthorized(httpContext))
                return true;

            return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            ViewResult result = new ViewResult();
            filterContext.HttpContext.Session["redirectUrl"] = string.Empty;

            if (!IsAuthorized(filterContext.HttpContext))
            {    
                string url = "~/Home";

                //try the cookie first
                HttpCookie cookie = filterContext.HttpContext.Request.Cookies[COACHCUE_AUTH_COOKIE];
                if (cookie != null)
                {
                    if (!string.IsNullOrEmpty(cookie.Values["userGUID"]))
                    {
                        string userGuid = cookie.Values["userGUID"].ToString();
                        string redirectURL =  (filterContext.HttpContext.Request.Url != null) ? filterContext.HttpContext.Request.Url.AbsoluteUri : "~/Home";

                        url = "~/Account/LoginByCookie?usr=" + userGuid + "&url=" + redirectURL;
                    }
                }

                filterContext.Result = new RedirectResult(url, false);
            }
        }

        private bool IsAuthorized(HttpContextBase httpContext)
        {
            bool auth = false;

            if (HttpContext.Current.User.Identity.IsAuthenticated)
                auth = true;
 
            return auth;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class ControlPanelAuthorizationAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            if (IsAuthorized())
                return true;

            return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            ViewResult result = new ViewResult();
            filterContext.HttpContext.Session["redirectUrl"] = string.Empty;

            if (!IsAuthorized())
            {    
                result.ViewName = "Login";
                filterContext.HttpContext.Session["redirectUrl"] = (filterContext.HttpContext.Request.Url != null) ? filterContext.HttpContext.Request.Url.AbsoluteUri : string.Empty;

                string url = "~/Home";
                filterContext.Result = new RedirectResult(url, false);
            }
        }

        private bool IsAuthorized()
        {
            bool auth = false;

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                user userItem = user.GetByEmail(HttpContext.Current.User.Identity.Name);
                if( userItem.isAdmin )
                    auth = true;
            }

            return auth;
        }
    }
}