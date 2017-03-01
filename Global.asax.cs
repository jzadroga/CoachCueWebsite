using CoachCue.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CoachCue
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DocumentDBRepository<CoachCue.Models.Player>.Initialize();
            DocumentDBRepository<CoachCue.Models.User>.Initialize();
            DocumentDBRepository<CoachCue.Models.Message>.Initialize();
            DocumentDBRepository<CoachCue.Models.Notification>.Initialize();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (Context.IsCustomErrorEnabled)
                ShowCustomErrorPage(Server.GetLastError());
        }

        private void ShowCustomErrorPage(Exception exception)
        {
            HttpException httpException = exception as HttpException;
            if (httpException == null)
                httpException = new HttpException(500, "Internal Server Error", exception);

           // Response.Clear();
            //Server.ClearError();

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;

            switch (httpException.GetHttpCode())
            {
                case 403:
                    routeData.Values.Add("action", "Index");
                    break;

                case 404:
                    routeData.Values.Add("action", "Index");
                    break;

                case 500:
                    routeData.Values.Add("action", "Index");
                    break;

                default:
                    routeData.Values.Add("action", "Index");
                    break;
            }

            Response.TrySkipIisCustomErrors = true;
            IController controller = new CoachCue.Controllers.ErrorController();
            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }
    }
}