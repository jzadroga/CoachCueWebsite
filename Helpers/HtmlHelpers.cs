using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoachCue.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string ActiveSection(this HtmlHelper helper, string controller/*, string action*/)
        {
            string classValue = string.Empty;

            string currentController = helper.ViewContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString();
            //string currentAction = helper.ViewContext.Controller.ValueProvider.GetValue("action").RawValue.ToString();

            if (currentController.ToLower() == controller.ToLower())// && currentAction == action)
            {
                classValue = "active";
            }

            return classValue;
        }
    }
}