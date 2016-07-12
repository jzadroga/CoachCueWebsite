using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoachCue.Helpers
{
    public static class ControllerExtensionsHelper
    {
        public static string PartialViewToString(this Controller controller)
        {
            return controller.PartialViewToString(null, null);
        }

        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return controller.PartialViewToString(viewName, null);
        }

        public static string RenderPartialViewToString(this Controller controller, object model)
        {
            return controller.PartialViewToString(null, model);
        }

        public static string PartialViewToString(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            }

            controller.ViewData.Model = model;

            using (StringWriter stringWriter = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
    }
}