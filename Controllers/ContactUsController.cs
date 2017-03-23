using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using System.ComponentModel;
using CoachCue.ViewModels;
using System.Net;
using System.IO;
using CoachCue.Helpers;

namespace CoachCue.Controllers
{
    public class ContactUsController : BaseController
    {
        public ActionResult Index()
        {
            return View(new PageViewModel());
        }

        [HttpPost]
        public JsonResult ContactUs(string email, string message)
        {
            string bodyMsg = "Message from: " + email;
            bodyMsg += " Message: " + message;

            string msg = EmailHelper.Send("info@coachcue.com", bodyMsg, "Contact from site", "Info");
            return Json(new { Result = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}
