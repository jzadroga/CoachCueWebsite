using System.Web;
using System.Web.Mvc;

namespace CoachCue
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
            //need to toggle this for debugging
            filters.Add(new RequireHttpsAttribute());
        }
    }
}   